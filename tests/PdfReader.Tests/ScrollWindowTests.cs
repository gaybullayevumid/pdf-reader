using PdfReader.Core;

namespace PdfReader.Tests;

public class ScrollWindowTests
{
    private const double Huge = 3_000_000_000; // about two million pages

    [Theory]
    [InlineData(0)]
    [InlineData(300_000)]
    [InlineData(500_000)]
    public void ShortDocumentNeverMovesTheWindow(double offset) =>
        Assert.Equal(0, ScrollWindow.Place(0, offset, 500_000));

    [Fact]
    public void WindowStaysWhileTheOffsetIsAwayFromItsEdges() =>
        Assert.Equal(1_000_000, ScrollWindow.Place(1_000_000, 1_300_000, Huge));

    [Theory]
    [InlineData(1_100_000)] // near the top edge
    [InlineData(1_500_000)] // near the bottom edge
    [InlineData(2_000_000_000)] // far outside
    public void WindowRecentersNearAnEdge(double offset) =>
        Assert.Equal(offset - ScrollWindow.Extent / 2, ScrollWindow.Place(1_000_000, offset, Huge));

    [Fact]
    public void WindowStopsAtTheDocumentStart() =>
        Assert.Equal(0, ScrollWindow.Place(1_000_000, 10, Huge));

    [Fact]
    public void WindowReachesTheDocumentEnd()
    {
        var start = ScrollWindow.Place(0, Huge - 700, Huge);
        Assert.Equal(Huge - ScrollWindow.Extent, start);
        Assert.Equal(start, ScrollWindow.Place(start, Huge - 700, Huge));
    }

    [Fact]
    public void WindowIsPulledBackWhenTheDocumentShrinks() =>
        Assert.Equal(400_000, ScrollWindow.Place(2_000_000, 900_000, 1_000_000));

    [Fact]
    public void PlacedOffsetIsAlwaysInsideTheWindow()
    {
        var start = 0.0;
        for (var offset = 0.0; offset < Huge; offset += 7_654_321)
        {
            start = ScrollWindow.Place(start, offset, Huge);
            Assert.InRange(offset - start, 0, ScrollWindow.Extent);
        }
    }
}
