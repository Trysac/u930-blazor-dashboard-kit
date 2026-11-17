namespace Trysac.BlazorDashboardKit;

/// <summary>
/// Evaluates a collection of <see cref="Filter"/> instances against tabular rows using logical AND.
/// </summary>
public static class Filters
{
    /// <summary>
    /// Maximum number of rows permitted in a single filter operation.
    /// </summary>
    public const int MaxRows = 10_000;

    /// <summary>
    /// Filters <paramref name="rows"/> by keeping all rows that satisfy every filter in <paramref name="filters"/>, preserving input order.
    /// </summary>
    /// <param name="rows">The input rows represented as field-to-value dictionaries.</param>
    /// <param name="filters">The filters combined with logical AND.</param>
    /// <returns>A read-only list of matching rows in their original order.</returns>
    /// <exception cref="DashboardException">
    /// Thrown with code <c>unknown_field</c> if a filter references a key missing from a row,
    /// <c>invalid_value</c> if a numeric filter encounters a non-numeric or non-finite value,
    /// or <c>too_many_points</c> if <paramref name="rows"/> exceeds <see cref="MaxRows"/>.
    /// </exception>
    public static IReadOnlyList<IReadOnlyDictionary<string, object?>> Apply(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        IEnumerable<Filter> filters)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(filters);

        if (rows.Count > MaxRows)
        {
            throw new DashboardException("too_many_points", $"Row count {rows.Count} exceeds maximum of {MaxRows}.");
        }

        var filterList = filters as IReadOnlyList<Filter> ?? filters.ToList();
        if (filterList.Count == 0)
        {
            return rows.ToList();
        }

        var result = new List<IReadOnlyDictionary<string, object?>>();
        foreach (var row in rows)
        {
            ArgumentNullException.ThrowIfNull(row);
            var keep = true;
            foreach (var filter in filterList)
            {
                ArgumentNullException.ThrowIfNull(filter);
                if (!row.TryGetValue(filter.Field, out var val))
                {
                    throw new DashboardException("unknown_field", $"Field '{filter.Field}' does not exist in row.");
                }

                if (!filter.Matches(val))
                {
                    keep = false;
                    break;
                }
            }

            if (keep)
            {
                result.Add(row);
            }
        }

        return result;
    }
}
