namespace Trysac.BlazorDashboardKit;

/// <summary>
/// Defines the color palette for rendering a chart, validated against WCAG 2.2 contrast requirements.
/// </summary>
/// <param name="Background">The hex color (#RRGGBB) of the chart background.</param>
/// <param name="Text">The hex color (#RRGGBB) of text labels and titles (requires >= 4.5 contrast vs background).</param>
/// <param name="SeriesColors">The hex colors (#RRGGBB) for data series (each requires >= 3.0 contrast vs background).</param>
public sealed record Theme(string Background, string Text, IReadOnlyList<string> SeriesColors)
{
    /// <summary>
    /// Gets the default high-contrast accessible theme whose colors all meet at least 4.5:1 contrast against #FFFFFF.
    /// </summary>
    public static Theme Default { get; } = new(
        "#FFFFFF",
        "#1F2937",
        new[]
        {
            "#1D4ED8",
            "#B45309",
            "#047857",
            "#7C3AED",
            "#BE185D"
        });
}
