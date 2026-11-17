using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace Trysac.BlazorDashboardKit.Tests;

/// <summary>
/// Property-based tests verifying invariants of Contrast, NiceTicks, and ChartRenderer.
/// </summary>
public class DashboardPropertiesTests
{
    private static readonly string[] PropCategories = ["C1", "C2"];

    [Property]
    public Property ContrastRatioIsSymmetricAndBetweenOneAndTwentyOne(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
    {
        var c1 = $"#{r1:X2}{g1:X2}{b1:X2}";
        var c2 = $"#{r2:X2}{g2:X2}{b2:X2}";

        var ratio12 = Contrast.Ratio(c1, c2);
        var ratio21 = Contrast.Ratio(c2, c1);

        var symmetric = Math.Abs(ratio12 - ratio21) < 1e-12;
        var inBounds = ratio12 >= 1.0 && ratio12 <= 21.0;
        return (symmetric && inBounds).ToProperty();
    }

    [Property]
    public Property NiceTicksEnclosesRangeAndIsStrictlyIncreasing(NormalFloat a, NormalFloat b)
    {
        var v1 = Math.Clamp(a.Get, -1e6, 1e6);
        var v2 = Math.Clamp(b.Get, -1e6, 1e6);
        var min = Math.Min(v1, v2);
        var max = Math.Max(v1, v2);

        var ticks = NiceTicks.Compute(min, max, 5);
        var coversBounds = ticks.Count >= 2 && ticks[0] <= min && ticks[^1] >= max;
        var strictlyIncreasing = true;
        for (var i = 1; i < ticks.Count; i++)
        {
            if (ticks[i] <= ticks[i - 1])
            {
                strictlyIncreasing = false;
                break;
            }
        }

        return (coversBounds && strictlyIncreasing).ToProperty();
    }

    [Property]
    public Property RenderSvgIsDeterministicForAnyFinitePair(NormalFloat a, NormalFloat b)
    {
        var v1 = Math.Clamp(a.Get, -10_000, 10_000);
        var v2 = Math.Clamp(b.Get, -10_000, 10_000);
        var spec = new ChartSpec(
            "Prop Title",
            "Prop Desc",
            PropCategories,
            [new Series("S1", [v1, v2])],
            ChartKind.Bar);

        var first = ChartRenderer.RenderSvg(spec);
        var second = ChartRenderer.RenderSvg(spec);
        return (first == second && first.StartsWith("<svg", StringComparison.Ordinal)).ToProperty();
    }
}
