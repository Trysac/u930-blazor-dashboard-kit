using System.Globalization;

namespace Trysac.BlazorDashboardKit;

/// <summary>
/// Represents a typed predicate over dictionary rows in a dataset.
/// </summary>
public abstract class Filter
{
    private protected Filter(string field)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            throw new DashboardException("unknown_field", "Filter field name must not be empty.");
        }

        Field = field;
    }

    /// <summary>
    /// Gets the name of the field inspected by this filter.
    /// </summary>
    public string Field { get; }

    /// <summary>
    /// Creates an equality filter matching rows whose field equals <paramref name="value"/>.
    /// </summary>
    /// <param name="field">The target field name.</param>
    /// <param name="value">The expected value.</param>
    /// <returns>A new equality <see cref="Filter"/>.</returns>
    public static Filter Equal(string field, object? value) => new EqualFilter(field, value);

    /// <summary>
    /// Creates an inclusive numeric range filter matching rows whose numeric field is within [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    /// <param name="field">The target field name.</param>
    /// <param name="min">The inclusive lower bound.</param>
    /// <param name="max">The inclusive upper bound.</param>
    /// <returns>A new range <see cref="Filter"/>.</returns>
    public static Filter Range(string field, double min, double max)
    {
        if (!double.IsFinite(min) || !double.IsFinite(max) || min > max)
        {
            throw new DashboardException("invalid_value", "Range bounds must be finite numbers with min <= max.");
        }

        return new RangeFilter(field, min, max);
    }

    /// <summary>
    /// Creates a set membership filter matching rows whose string field is in <paramref name="values"/>.
    /// </summary>
    /// <param name="field">The target field name.</param>
    /// <param name="values">The allowed string values.</param>
    /// <returns>A new set membership <see cref="Filter"/>.</returns>
    public static Filter In(string field, params string[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return new InFilter(field, values);
    }

    internal abstract bool Matches(object? value);

    internal static bool TryConvertDouble(object? value, out double number)
    {
        switch (value)
        {
            case double d:
                if (!double.IsFinite(d))
                {
                    throw new DashboardException("invalid_value", "Row contains a non-finite double value.");
                }

                number = d;
                return true;
            case float f:
                if (!float.IsFinite(f))
                {
                    throw new DashboardException("invalid_value", "Row contains a non-finite float value.");
                }

                number = f;
                return true;
            case int i:
                number = i;
                return true;
            case long l:
                number = l;
                return true;
            case decimal m:
                number = (double)m;
                return true;
            case short s:
                number = s;
                return true;
            case byte b:
                number = b;
                return true;
            default:
                number = 0;
                return false;
        }
    }

    private sealed class EqualFilter : Filter
    {
        private readonly object? _expected;

        public EqualFilter(string field, object? expected)
            : base(field)
        {
            if (expected is double d && !double.IsFinite(d))
            {
                throw new DashboardException("invalid_value", "Filter value must be finite.");
            }

            _expected = expected;
        }

        internal override bool Matches(object? value)
        {
            if (_expected is null && value is null)
            {
                return true;
            }

            if (_expected is null || value is null)
            {
                return false;
            }

            if (TryConvertDouble(_expected, out var expectedNum) && TryConvertDouble(value, out var actualNum))
            {
                return expectedNum == actualNum;
            }

            return Equals(_expected, value) ||
                   string.Equals(
                       Convert.ToString(_expected, CultureInfo.InvariantCulture),
                       Convert.ToString(value, CultureInfo.InvariantCulture),
                       StringComparison.Ordinal);
        }
    }

    private sealed class RangeFilter : Filter
    {
        private readonly double _min;
        private readonly double _max;

        public RangeFilter(string field, double min, double max)
            : base(field)
        {
            _min = min;
            _max = max;
        }

        internal override bool Matches(object? value)
        {
            if (!TryConvertDouble(value, out var num))
            {
                throw new DashboardException("invalid_value", $"Field '{Field}' does not contain a numeric value.");
            }

            return num >= _min && num <= _max;
        }
    }

    private sealed class InFilter : Filter
    {
        private readonly HashSet<string> _allowed;

        public InFilter(string field, string[] values)
            : base(field)
        {
            _allowed = new HashSet<string>(values, StringComparer.Ordinal);
        }

        internal override bool Matches(object? value)
        {
            return value is string s && _allowed.Contains(s);
        }
    }
}
