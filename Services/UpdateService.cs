using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using PdfReader.Core;
using PdfReader.Models;

namespace PdfReader.Services;

public interface IUpdateService
{
    Version CurrentVersion { get; }

    /// <summary>Newest release found by the last check, if it is newer than the running version.</summary>
    UpdateInfo? Available { get; }

    event EventHandler? AvailableChanged;

    Task<UpdateCheckResult> CheckAsync(CancellationToken ct = default);

    /// <summary>Downloads the installer, starts it silently and closes the app so it can be replaced.</summary>
    Task InstallAsync(IProgress<double>? progress = null, CancellationToken ct = default);
}

/// <summary>Checks GitHub Releases for a newer version and runs its installer.</summary>
public sealed class UpdateService(ILogger<UpdateService> logger, IWindowProvider windows) : IUpdateService
{
    private const string LatestReleaseUrl = "https://api.github.com/repos/gaybullayevumid/pdf-reader/releases/latest";
    private const string OverrideEnvVar = "PDFREADER_UPDATE_URL"; // for testing against a local server

    private static readonly HttpClient Http = CreateClient();
    private static readonly string[] TrustedHosts = ["github.com", "githubusercontent.com"];

    public Version CurrentVersion { get; } = ReleaseParser.Normalize(typeof(UpdateService).Assembly.GetName().Version);

    public UpdateInfo? Available { get; private set; }

    public event EventHandler? AvailableChanged;

    public async Task<UpdateCheckResult> CheckAsync(CancellationToken ct = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(15));

            using var response = await Http.GetAsync(OverrideUrl ?? LatestReleaseUrl, cts.Token);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                SetAvailable(null); // no releases published yet
                return UpdateCheckResult.UpToDate;
            }
            response.EnsureSuccessStatusCode();

            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cts.Token), cancellationToken: cts.Token);
            var update = ReleaseParser.Parse(json.RootElement, IsTrusted);
            if (update is null || update.Version <= CurrentVersion)
            {
                SetAvailable(null);
                return UpdateCheckResult.UpToDate;
            }

            SetAvailable(update);
            return UpdateCheckResult.Available;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Update check failed");
            return UpdateCheckResult.Failed;
        }
    }

    public async Task InstallAsync(IProgress<double>? progress = null, CancellationToken ct = default)
    {
        // The release found when the app started may have been superseded since (the app can stay open for hours),
        // so look again and install the newest one. If the check fails, the release already found is used.
        await CheckAsync(ct);
        var update = Available ?? throw new InvalidOperationException("No update available.");
        var path = await DownloadAsync(update, progress, ct);

        // The installer writes a log next to itself, so a failed silent update can be diagnosed.
        var log = Path.Combine(Path.GetDirectoryName(path)!, "install.log");
        logger.LogInformation("Starting installer {Path} for {Version} (log: {Log})", path, update.Version, log);
        Process.Start(new ProcessStartInfo(path, $"/SILENT /SUPPRESSMSGBOXES /NORESTART /CLOSEAPPLICATIONS /LAUNCH=1 /LOG=\"{log}\"")
        {
            UseShellExecute = true, // lets Windows show the elevation prompt if the install is per-machine
        });
        ExitForUpdate();
    }

    /// <summary>
    /// The installer cannot replace the files of a process that is still running, and a WinUI process has been seen
    /// to linger without a window after <see cref="Application.Exit"/>, which made the silent update fail. So the
    /// app shuts down normally (closing the window saves the window state and open tabs), then ends for certain.
    /// </summary>
    private void ExitForUpdate()
    {
        try
        {
            windows.Window.Close();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Closing the window before the update failed");
        }
        logger.LogInformation("Exiting for the update");
        Serilog.Log.CloseAndFlush();
        Environment.Exit(0);
    }

    private async Task<string> DownloadAsync(UpdateInfo update, IProgress<double>? progress, CancellationToken ct)
    {
        var directory = Path.Combine(Path.GetTempPath(), "PdfReader-Update");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, update.InstallerName);

        using var response = await Http.GetAsync(update.InstallerUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        var total = response.Content.Headers.ContentLength;

        await using (var source = await response.Content.ReadAsStreamAsync(ct))
        await using (var target = File.Create(path))
        {
            var buffer = new byte[81920];
            long done = 0;
            int read;
            while ((read = await source.ReadAsync(buffer, ct)) > 0)
            {
                await target.WriteAsync(buffer.AsMemory(0, read), ct);
                done += read;
                if (total > 0) progress?.Report((double)done / total.Value);
            }
        }

        if (update.Sha256 is not null)
        {
            string hash;
            await using (var file = File.OpenRead(path))
                hash = Convert.ToHexString(await SHA256.HashDataAsync(file, ct));
            if (!hash.Equals(update.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(path);
                throw new InvalidDataException("The downloaded installer did not match its published checksum.");
            }
        }
        return path;
    }

    private static bool IsTrusted(string url)
    {
        if (OverrideUrl is not null) return true; // test mode
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps) return false;
        return TrustedHosts.Any(h => uri.Host == h || uri.Host.EndsWith("." + h, StringComparison.OrdinalIgnoreCase));
    }

    private static string? OverrideUrl => Environment.GetEnvironmentVariable(OverrideEnvVar) is { Length: > 0 } v ? v : null;

    private void SetAvailable(UpdateInfo? update)
    {
        if (Equals(Available, update)) return;
        Available = update;
        AvailableChanged?.Invoke(this, EventArgs.Empty);
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PdfReader", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }
}
