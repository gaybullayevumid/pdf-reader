using PdfReader.Core;

namespace PdfReader.Tests;

public class WindowPlacementTests
{
    private static readonly RectI Primary = new(0, 0, 1920, 1040);
    private static readonly RectI Second = new(1920, 0, 1920, 1040);
    private static readonly RectI Left = new(-1600, 100, 1600, 900);

    [Fact]
    public void Default_state_is_centered_on_the_primary_display()
    {
        var result = WindowPlacement.Sanitize(null, [Primary]);

        Assert.Equal(WindowMode.Normal, result.Mode);
        Assert.Equal(WindowPlacement.DefaultWidth, result.Width);
        Assert.Equal(WindowPlacement.DefaultHeight, result.Height);
        Assert.Equal((1920 - 1280) / 2, result.X);
        Assert.Equal((1040 - 820) / 2, result.Y);
    }

    [Fact]
    public void A_valid_position_is_kept()
    {
        var saved = new WindowStateData(WindowMode.Normal, 200, 150, 1000, 700);
        Assert.Equal(saved, WindowPlacement.Sanitize(saved, [Primary]));
    }

    [Theory]
    [InlineData(WindowMode.FullScreen)]
    [InlineData(WindowMode.Maximized)]
    public void Fullscreen_and_maximized_modes_are_preserved(WindowMode mode)
    {
        var saved = new WindowStateData(mode, 100, 100, 1100, 700, WasMaximized: mode == WindowMode.Maximized);
        var result = WindowPlacement.Sanitize(saved, [Primary]);

        Assert.Equal(mode, result.Mode);
        Assert.Equal(1100, result.Width);
        Assert.Equal(700, result.Height);
    }

    [Fact]
    public void Was_maximized_flag_survives()
    {
        var result = WindowPlacement.Sanitize(new WindowStateData(WindowMode.FullScreen, 10, 10, 900, 600, WasMaximized: true), [Primary]);
        Assert.True(result.WasMaximized);
        Assert.Equal(WindowMode.FullScreen, result.Mode);
    }

    [Fact]
    public void A_window_on_a_disconnected_monitor_is_moved_back_on_screen()
    {
        var saved = new WindowStateData(WindowMode.Normal, 5000, 300, 1000, 700);
        var result = WindowPlacement.Sanitize(saved, [Primary]);

        Assert.Equal((1920 - 1000) / 2, result.X);
        Assert.Equal((1040 - 700) / 2, result.Y);
    }

    [Fact]
    public void A_window_on_a_second_monitor_stays_there()
    {
        var saved = new WindowStateData(WindowMode.Normal, 2100, 100, 1000, 700);
        var result = WindowPlacement.Sanitize(saved, [Primary, Second]);

        Assert.Equal(2100, result.X);
        Assert.Equal(100, result.Y);
    }

    [Fact]
    public void A_window_on_a_monitor_left_of_the_primary_keeps_negative_coordinates()
    {
        var saved = new WindowStateData(WindowMode.Normal, -1500, 200, 1000, 700);
        var result = WindowPlacement.Sanitize(saved, [Primary, Left]);

        Assert.Equal(-1500, result.X);
        Assert.Equal(200, result.Y);
    }

    [Fact]
    public void A_window_barely_visible_at_a_corner_is_recentered()
    {
        var saved = new WindowStateData(WindowMode.Normal, 1900, 1020, 1000, 700);
        var result = WindowPlacement.Sanitize(saved, [Primary]);

        Assert.Equal((1920 - 1000) / 2, result.X);
    }

    [Fact]
    public void A_window_hanging_over_the_edge_is_pulled_fully_inside()
    {
        var saved = new WindowStateData(WindowMode.Normal, 1400, 600, 1000, 700);
        var result = WindowPlacement.Sanitize(saved, [Primary]);

        Assert.Equal(1920 - 1000, result.X);
        Assert.Equal(1040 - 700, result.Y);
    }

    [Fact]
    public void An_oversized_window_is_shrunk_to_the_display()
    {
        var saved = new WindowStateData(WindowMode.Normal, 0, 0, 5000, 4000);
        var result = WindowPlacement.Sanitize(saved, [Primary]);

        Assert.Equal(1920, result.Width);
        Assert.Equal(1040, result.Height);
        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-300, -10)]
    [InlineData(int.MaxValue, int.MaxValue)]
    [InlineData(10, 10)]
    public void Invalid_or_tiny_sizes_become_safe(int width, int height)
    {
        var result = WindowPlacement.Sanitize(new WindowStateData(WindowMode.Normal, 50, 50, width, height), [Primary]);

        Assert.InRange(result.Width, WindowPlacement.MinWidth, Primary.Width);
        Assert.InRange(result.Height, WindowPlacement.MinHeight, Primary.Height);
    }

    [Fact]
    public void An_unknown_mode_value_becomes_normal()
    {
        var result = WindowPlacement.Sanitize(new WindowStateData((WindowMode)42, 100, 100, 1000, 700), [Primary]);
        Assert.Equal(WindowMode.Normal, result.Mode);
    }

    [Fact]
    public void Without_any_display_information_only_the_size_is_validated()
    {
        var result = WindowPlacement.Sanitize(new WindowStateData(WindowMode.Normal, 100, 100, -5, 700), []);

        Assert.Equal(WindowPlacement.DefaultWidth, result.Width);
        Assert.Null(result.X);
        Assert.Null(result.Y);
    }

    [Fact]
    public void A_tiny_display_gets_a_window_that_fits()
    {
        var result = WindowPlacement.Sanitize(null, [new RectI(0, 0, 600, 400)]);

        Assert.True(result.Width <= 600);
        Assert.True(result.Height <= 400);
    }

    [Fact]
    public void Sanitizing_twice_changes_nothing()
    {
        var saved = new WindowStateData(WindowMode.Normal, 5000, -400, 99999, 50);
        var once = WindowPlacement.Sanitize(saved, [Primary, Second]);

        Assert.Equal(once, WindowPlacement.Sanitize(once, [Primary, Second]));
    }

    [Fact]
    public void Rect_intersection()
    {
        Assert.Equal(new RectI(50, 50, 50, 50), new RectI(0, 0, 100, 100).Intersect(new RectI(50, 50, 100, 100)));
        Assert.Null(new RectI(0, 0, 10, 10).Intersect(new RectI(10, 0, 10, 10)));
    }
}
