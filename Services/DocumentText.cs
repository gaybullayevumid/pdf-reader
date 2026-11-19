using Microsoft.Extensions.Logging;
using PdfReader.Core;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Actions;
using UglyToad.PdfPig.Annotations;
using UglyToad.PdfPig.Content;

namespace PdfReader.Services;

/// <summary>
/// The text layer of an open document, read with PdfPig (Windows.Data.Pdf can only render). The file is parsed on
/// first use and pages are extracted on demand, off the UI thread, and cached. A document that cannot be parsed
/// simply has no text.
/// </summary>
public sealed class DocumentText : IDisposable
{
    private const int CacheSize = 64;

    private readonly string _path;
    private readonly string? _password;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _gate = new(1); // PdfPig documents are not thread-safe
    private readonly Dictionary<int, PageText> _cache = [];
    private readonly Queue<int> _cacheOrder = new();
    private PdfDocument? _doc;
    private bool _failed;
    private bool _disposed;

    public DocumentText(string path, string? password, ILogger logger)
    {
        _path = path;
        _password = password;
        _logger = logger;
    }

    /// <summary>The page's text if it has already been extracted.</summary>
    public PageText? TryGet(int page)
    {
        lock (_cache) return _cache.GetValueOrDefault(page);
    }

    public async Task<PageText> GetAsync(int page)
    {
        if (TryGet(page) is { } cached) return cached;

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (TryGet(page) is { } raced) return raced;
            var text = await Task.Run(() => Extract(page)).ConfigureAwait(false);
            lock (_cache)
            {
                if (_cache.TryAdd(page, text)) _cacheOrder.Enqueue(page);
                while (_cacheOrder.Count > CacheSize) _cache.Remove(_cacheOrder.Dequeue());
            }
            return text;
        }
        finally
        {
            _gate.Release();
        }
    }

    private PageText Extract(int index)
    {
        if (_disposed || _failed) return PageText.Empty;
        try
        {
            _doc ??= PdfDocument.Open(_path, new ParsingOptions { Password = _password ?? string.Empty });
            var page = _doc.GetPage(index + 1);
            return PageText.Build(ToGlyphs(page), ToLinks(page, index));
        }
        catch (Exception ex) when (_doc is null)
        {
            _failed = true;
            _logger.LogWarning(ex, "Text layer unavailable for {Path}", _path);
            return PageText.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read the text of page {Page}", index + 1);
            return PageText.Empty;
        }
    }

    /// <summary>
    /// PdfPig reports letters in the displayed (rotated) page space with a bottom-left origin; glyphs use page
    /// fractions from the top-left, so they line up with the rendered bitmap at any zoom.
    /// </summary>
    private static IEnumerable<Glyph> ToGlyphs(Page page)
    {
        var width = page.Width;
        var height = page.Height;
        if (width <= 0 || height <= 0) yield break;

        // Unrotated pages may have a crop box that does not start at the origin.
        var originX = page.Rotation.Value == 0 ? page.CropBox.Bounds.Left : 0;
        var originY = page.Rotation.Value == 0 ? page.CropBox.Bounds.Bottom : 0;

        foreach (var letter in page.Letters)
        {
            var box = letter.BoundingBox;
            var size = letter.PointSize > 0 ? letter.PointSize : Math.Max(1, box.Height);
            var baseline = letter.StartBaseLine.Y;

            // Selection boxes span the font's ascent and descent, so a line highlights evenly and spaces have size.
            var left = Math.Min(box.Left, letter.StartBaseLine.X);
            var right = Math.Max(box.Right, letter.EndBaseLine.X);
            var top = Math.Max(box.Top, baseline + 0.8 * size);
            var bottom = Math.Min(box.Bottom, baseline - 0.25 * size);

            yield return new Glyph(letter.Value, new TextRect(
                (left - originX) / width,
                1 - (top - originY) / height,
                (right - originX) / width,
                1 - (bottom - originY) / height));
        }
    }

    /// <summary>
    /// The page's link annotations in the same page fractions as glyphs. Only web and mail addresses and jumps within
    /// the document are kept; a page whose annotations cannot be read just has no links.
    /// </summary>
    private List<PageLink> ToLinks(Page page, int index)
    {
        var links = new List<PageLink>();
        var width = page.Width;
        var height = page.Height;
        if (width <= 0 || height <= 0) return links;
        var originX = page.Rotation.Value == 0 ? page.CropBox.Bounds.Left : 0;
        var originY = page.Rotation.Value == 0 ? page.CropBox.Bounds.Bottom : 0;

        try
        {
            foreach (var annotation in page.GetAnnotations())
            {
                if (annotation.Type != AnnotationType.Link) continue;

                string? uri = null;
                int? target = null;
                double? top = null;
                switch (annotation.Action)
                {
                    case UriAction { Uri: { } address } when IsOpenable(address):
                        uri = address.Trim();
                        break;
                    case GoToAction { Destination: { PageNumber: > 0 } destination }:
                        target = destination.PageNumber - 1;
                        top = destination.Coordinates?.Top;
                        break;
                    default:
                        continue;
                }

                var r = annotation.Rectangle;
                var box = new TextRect(
                    (Math.Min(r.Left, r.Right) - originX) / width,
                    1 - (Math.Max(r.Top, r.Bottom) - originY) / height,
                    (Math.Max(r.Left, r.Right) - originX) / width,
                    1 - (Math.Min(r.Top, r.Bottom) - originY) / height);
                links.Add(new PageLink(box, uri, target, top));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read the links of page {Page}", index + 1);
        }
        return links;
    }

    /// <summary>Only addresses a browser or mail client handles; file:, javascript: and the like are ignored.</summary>
    private static bool IsOpenable(string address) =>
        System.Uri.TryCreate(address.Trim(), UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https" or "mailto";

    public void Dispose()
    {
        _disposed = true;
        // Wait for a running extraction so the document is not closed under it.
        _ = Task.Run(async () =>
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            _doc?.Dispose();
            _doc = null;
        });
    }
}
