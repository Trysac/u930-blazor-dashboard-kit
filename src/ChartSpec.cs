namespace Trysac.BlazorDashboardKit;

/// <summary>
/// Defines the data, labels, kind, dimensions, and accessibility identifier prefix for a chart.
/// </summary>
/// <param name="Title">The accessible chart title.</param>
/// <param name="Description">The accessible chart description.</param>
/// <param name="Categories">The ordered category labels along the horizontal axis.</param>
/// <param name="Series">The data series to plot.</param>
/// <param name="Kind">The chart visual kind (<see cref="ChartKind.Bar"/> or <see cref="ChartKind.Line"/>).</param>
/// <param name="Width">The total SVG width in pixels (default 640).</param>
/// <param name="Height">The total SVG height in pixels (default 360).</param>
/// <param name="IdPrefix">The unique element ID prefix for ARIA references (default "chart").</param>
public sealed record ChartSpec(
    string Title,
    string Description,
    IReadOnlyList<string> Categories,
    IReadOnlyList<Series> Series,
    ChartKind Kind,
    int Width = 640,
    int Height = 360,
    string IdPrefix = "chart");
