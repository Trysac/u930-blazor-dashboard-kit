namespace Trysac.BlazorDashboardKit;

/// <summary>
/// Represents a named data series in a chart.
/// </summary>
/// <param name="Name">The display name of the series.</param>
/// <param name="Values">The numeric data values corresponding to each category.</param>
public sealed record Series(string Name, IReadOnlyList<double> Values);
