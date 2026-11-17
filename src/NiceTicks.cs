namespace Trysac.BlazorDashboardKit;

/// <summary>
/// Implements Paul Heckbert's (1990, Graphics Gems) nice numbers algorithm for human-readable axis ticks.
/// </summary>
public static class NiceTicks
{
    /// <summary>
    /// Computes a sequence of evenly spaced nice tick values covering the range <paramref name="min"/> to <paramref name="max"/>.
    /// </summary>
    /// <param name="min">The minimum data value.</param>
    /// <param name="max">The maximum data value.</param>
    /// <param name="count">The desired number of ticks (must be between 2 and 20 inclusive).</param>
    /// <returns>An ordered read-only list of tick values.</returns>
    /// <exception cref="DashboardException">
    /// Thrown with code <c>invalid_value</c> if <paramref name="min"/> or <paramref name="max"/> is NaN or infinite,
    /// or <c>invalid_ticks</c> if <paramref name="count"/> is outside 2..20 or <paramref name="min"/> &gt; <paramref name="max"/>.
    /// </exception>
    public static IReadOnlyList<double> Compute(double min, double max, int count)
    {
        if (!double.IsFinite(min) || !double.IsFinite(max))
        {
            throw new DashboardException("invalid_value", "Tick bounds must be finite numbers.");
        }

        if (count is < 2 or > 20 || min > max)
        {
            throw new DashboardException("invalid_ticks", "Tick count must be between 2 and 20 and min must not exceed max.");
        }

        if (min == max)
        {
            min -= 1.0;
            max += 1.0;
        }

        var range = NiceNum(max - min, round: false);
        var d = NiceNum(range / (count - 1), round: true);
        var graphMin = Math.Floor(min / d) * d;
        var graphMax = Math.Ceiling(max / d) * d;

        var nfrac = (int)Math.Max(-Math.Floor(Math.Log10(d)), 0);
        if (nfrac > 15)
        {
            nfrac = 15;
        }

        var steps = (int)Math.Round((graphMax - graphMin) / d);
        var ticks = new List<double>(steps + 1);
        for (var i = 0; i <= steps; i++)
        {
            var val = graphMin + (i * d);
            val = Math.Round(val, nfrac, MidpointRounding.AwayFromZero);
            if (val == 0.0)
            {
                val = 0.0;
            }

            ticks.Add(val);
        }

        return ticks;
    }

    private static double NiceNum(double x, bool round)
    {
        var exp = Math.Floor(Math.Log10(x));
        var f = x / Math.Pow(10.0, exp);
        double nf;
        if (round)
        {
            if (f < 1.5)
            {
                nf = 1.0;
            }
            else if (f < 3.0)
            {
                nf = 2.0;
            }
            else if (f < 7.0)
            {
                nf = 5.0;
            }
            else
            {
                nf = 10.0;
            }
        }
        else
        {
            if (f <= 1.0)
            {
                nf = 1.0;
            }
            else if (f <= 2.0)
            {
                nf = 2.0;
            }
            else if (f <= 5.0)
            {
                nf = 5.0;
            }
            else
            {
                nf = 10.0;
            }
        }

        return nf * Math.Pow(10.0, exp);
    }
}
