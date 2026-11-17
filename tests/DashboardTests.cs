using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace Trysac.BlazorDashboardKit.Tests;

/// <summary>
/// Unit and acceptance tests for ChartRenderer, NiceTicks, Contrast, Filters, and DashboardException.
/// </summary>
public class DashboardTests
{
    private static readonly string[] ThreeMonths = ["Jan", "Feb", "Mar"];
    private static readonly double[] Sales2026 = [120, 80, 150];
    private static readonly double[] ExpectedTicks1 = [0, 20, 40, 60, 80, 100];
    private static readonly double[] ExpectedTicks2 = [-5, 0, 5, 10];
    private static readonly string[] CategoriesAB = ["A", "B"];
    private static readonly double[] Values1020 = [10, 20];
    private static readonly string[] YellowSeries = ["#FFFF00"];
    private static readonly string[] BlueSeries = ["#1D4ED8"];
    private static readonly string[] RegionFilterValues = ["north", "south"];
    private static readonly string[] CategoriesJanFeb = ["Jan", "Feb"];
    private static readonly double[] SingleValueOne = [1.0];
    private static readonly string[] SingleCategoryJan = ["Jan"];
    private static readonly double[] NanValueArray = [double.NaN];
    private static readonly double[] NegInfValueArray = [double.NegativeInfinity];
    private static readonly string[] BadIdPrefixes = ["", "1chart", "Chart", "chart_1", "a01234567890123456789012345678901"];
    private static readonly double[] SingleValOne = [1];
    private static readonly double[] SingleValTwo = [2];
    private static readonly string[] SingleCategoryA = ["A"];
    private static readonly string[] CategoriesQ1Q2 = ["Q1", "Q2"];
    private static readonly double[] GrowthValues = [1.5, 2.75];
    private static readonly double[] ProfitLossValues = [-25, 0, 40];
    private static readonly double[] LossOnlyValues = [-10, -30];
    private static readonly double[] FiftyValue = [50.0];
    private static readonly string[] SingleCategoryOnlyCat = ["OnlyCat"];

    [Fact]
    public void AcceptanceBarChartMonthlySalesRendersAccessibleSvgWithTitlesAndTicks()
    {
        var spec = new ChartSpec(
            Title: "Monthly sales",
            Description: "Bar chart of monthly sales for 2026",
            Categories: ThreeMonths,
            Series: [new Series("2026", Sales2026)],
            Kind: ChartKind.Bar);

        var svg = ChartRenderer.RenderSvg(spec);

        Assert.StartsWith("<svg", svg, StringComparison.Ordinal);
        Assert.Contains("role=\"img\"", svg, StringComparison.Ordinal);
        Assert.Contains("aria-labelledby=\"chart-title chart-desc\"", svg, StringComparison.Ordinal);
        Assert.Contains("<title id=\"chart-title\">Monthly sales</title>", svg, StringComparison.Ordinal);

        var barCount = Regex.Count(svg, "<rect class=\"bar\"");
        Assert.Equal(3, barCount);

        Assert.Contains("<title>Jan, 2026: 120</title>", svg, StringComparison.Ordinal);
        Assert.Contains("<title>Feb, 2026: 80</title>", svg, StringComparison.Ordinal);
        Assert.Contains("<title>Mar, 2026: 150</title>", svg, StringComparison.Ordinal);

        var expectedTicks = NiceTicks.Compute(0, 150, 5);
        foreach (var tick in expectedTicks)
        {
            var label = tick.ToString("0.##", CultureInfo.InvariantCulture);
            Assert.Contains($">{label}</text>", svg, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void AcceptanceNiceTicksComputesHeckbertNumbers()
    {
        var ticks1 = NiceTicks.Compute(0, 97, 5);
        Assert.Equal(ExpectedTicks1, ticks1);

        var ticks2 = NiceTicks.Compute(-3.2, 7.9, 5);
        Assert.Equal(ExpectedTicks2, ticks2);
    }

    [Fact]
    public void NiceTicksEdgeCasesEqualMinMaxAndInvalidInputs()
    {
        var equalTicks = NiceTicks.Compute(42, 42, 5);
        Assert.True(equalTicks[0] <= 41.0);
        Assert.True(equalTicks[^1] >= 43.0);

        // Cover other nicenum branches (f < 3, f <= 2, f > 5, etc.)
        var branchTicksA = NiceTicks.Compute(0, 18, 5);
        Assert.Equal(0, branchTicksA[0]);
        var branchTicksB = NiceTicks.Compute(0, 65, 4);
        Assert.Equal(0, branchTicksB[0]);
        var branchTicksC = NiceTicks.Compute(0, 8.5, 5);
        Assert.Equal(0, branchTicksC[0]);

        var lowCount = Assert.Throws<DashboardException>(() => NiceTicks.Compute(0, 10, 1));
        Assert.Equal("invalid_ticks", lowCount.Code);

        var highCount = Assert.Throws<DashboardException>(() => NiceTicks.Compute(0, 10, 21));
        Assert.Equal("invalid_ticks", highCount.Code);

        var inverted = Assert.Throws<DashboardException>(() => NiceTicks.Compute(10, 5, 5));
        Assert.Equal("invalid_ticks", inverted.Code);

        var nanErr = Assert.Throws<DashboardException>(() => NiceTicks.Compute(double.NaN, 10, 5));
        Assert.Equal("invalid_value", nanErr.Code);

        var infErr = Assert.Throws<DashboardException>(() => NiceTicks.Compute(0, double.PositiveInfinity, 5));
        Assert.Equal("invalid_value", infErr.Code);
    }

    [Fact]
    public void AcceptanceContrastComputesWcagLuminanceAndChecksDefaultTheme()
    {
        Assert.Equal(21.0, Math.Round(Contrast.Ratio("#000000", "#FFFFFF"), 2));
        Assert.Equal(4.54, Math.Round(Contrast.Ratio("#767676", "#FFFFFF"), 2));

        Assert.True(Contrast.Ratio(Theme.Default.Text, Theme.Default.Background) >= 4.5);
        foreach (var seriesColor in Theme.Default.SeriesColors)
        {
            Assert.True(Contrast.Ratio(seriesColor, Theme.Default.Background) >= 4.5);
        }

        // Also verify lowercase hex parsing
        Assert.Equal(21.0, Math.Round(Contrast.Ratio("#ffffff", "#000000"), 2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("#FFF")]
    [InlineData("FFFFFF")]
    [InlineData("#GGGGGG")]
    [InlineData("#12345G")]
    public void ContrastRejectsInvalidColorFormats(string? invalidColor)
    {
        var ex = Assert.Throws<DashboardException>(() => Contrast.Ratio(invalidColor!, "#FFFFFF"));
        Assert.Equal("invalid_color", ex.Code);
    }

    [Fact]
    public void AcceptanceLowContrastThemeThrowsDashboardException()
    {
        var spec = new ChartSpec(
            Title: "Contrast check",
            Description: "Testing contrast rejection",
            Categories: CategoriesAB,
            Series: [new Series("S1", Values1020)],
            Kind: ChartKind.Bar);

        var badSeriesTheme = new Theme("#FFFFFF", "#1F2937", YellowSeries);
        var exSeries = Assert.Throws<DashboardException>(() => ChartRenderer.RenderSvg(spec, badSeriesTheme));
        Assert.Equal("low_contrast", exSeries.Code);

        var badTextTheme = new Theme("#FFFFFF", "#999999", BlueSeries);
        var exText = Assert.Throws<DashboardException>(() => ChartRenderer.RenderSvg(spec, badTextTheme));
        Assert.Equal("low_contrast", exText.Code);
    }

    [Fact]
    public void AcceptanceRenderTableAndHtmlEscapingInSvgAndTable()
    {
        var spec = new ChartSpec(
            Title: "Monthly sales",
            Description: "Sales table",
            Categories: ThreeMonths,
            Series: [new Series("2026", Sales2026)],
            Kind: ChartKind.Bar);

        var table = ChartRenderer.RenderTable(spec);
        Assert.StartsWith("<table>", table, StringComparison.Ordinal);
        Assert.Contains("<caption>Monthly sales</caption>", table, StringComparison.Ordinal);
        Assert.Contains("<th scope=\"col\">2026</th>", table, StringComparison.Ordinal);
        Assert.Contains("<th scope=\"row\">Jan</th>", table, StringComparison.Ordinal);

        var escapingSpec = spec with { Title = "<b>&" };
        var escapedSvg = ChartRenderer.RenderSvg(escapingSpec);
        var escapedTable = ChartRenderer.RenderTable(escapingSpec);

        Assert.Contains("&lt;b&gt;&amp;", escapedSvg, StringComparison.Ordinal);
        Assert.Contains("&lt;b&gt;&amp;", escapedTable, StringComparison.Ordinal);
    }

    [Fact]
    public void AcceptanceFiltersRangeAndInAndUnknownField()
    {
        var rows = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["id"] = 1, ["amount"] = 10.0, ["region"] = "north" },
            new Dictionary<string, object?> { ["id"] = 2, ["amount"] = 25.0, ["region"] = "north" },
            new Dictionary<string, object?> { ["id"] = 3, ["amount"] = 15, ["region"] = "south" },
            new Dictionary<string, object?> { ["id"] = 4, ["amount"] = 20.0, ["region"] = "west" },
            new Dictionary<string, object?> { ["id"] = 5, ["amount"] = 20.0, ["region"] = "north" }
        };

        var filtered = Filters.Apply(
            rows,
            [
                Filter.Range("amount", 10, 20),
                Filter.In("region", RegionFilterValues)
            ]);

        Assert.Equal(3, filtered.Count);
        Assert.Equal(1, filtered[0]["id"]);
        Assert.Equal(3, filtered[1]["id"]);
        Assert.Equal(5, filtered[2]["id"]);

        var ex = Assert.Throws<DashboardException>(() =>
            Filters.Apply(rows, [Filter.Equal("missing_col", "val")]));
        Assert.Equal("unknown_field", ex.Code);
    }

    [Fact]
    public void FiltersEqualNumericTypesAndEdgeCases()
    {
        var rows = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["a"] = 10L, ["b"] = 1.5f, ["c"] = 20m, ["d"] = (short)5, ["e"] = (byte)2, ["n"] = null },
            new Dictionary<string, object?> { ["a"] = 99L, ["b"] = 2.5f, ["c"] = 30m, ["d"] = (short)6, ["e"] = (byte)3, ["n"] = "not-null" }
        };

        Assert.Single(Filters.Apply(rows, [Filter.Equal("a", 10)]));
        Assert.Single(Filters.Apply(rows, [Filter.Equal("b", 1.5)]));
        Assert.Single(Filters.Apply(rows, [Filter.Equal("c", 20.0)]));
        Assert.Single(Filters.Apply(rows, [Filter.Equal("d", 5)]));
        Assert.Single(Filters.Apply(rows, [Filter.Equal("e", 2)]));
        Assert.Single(Filters.Apply(rows, [Filter.Equal("n", null)]));
        Assert.Single(Filters.Apply(rows, [Filter.Equal("n", "not-null")]));
        Assert.Equal(2, Filters.Apply(rows, Array.Empty<Filter>()).Count);

        var nonNumericRange = Assert.Throws<DashboardException>(() =>
            Filters.Apply(rows, [Filter.Range("n", 0, 10)]));
        Assert.Equal("invalid_value", nonNumericRange.Code);

        var blankField = Assert.Throws<DashboardException>(() => Filter.Equal("  ", 1));
        Assert.Equal("unknown_field", blankField.Code);

        var badRange = Assert.Throws<DashboardException>(() => Filter.Range("a", 20, 10));
        Assert.Equal("invalid_value", badRange.Code);

        var nanEqual = Assert.Throws<DashboardException>(() => Filter.Equal("a", double.NaN));
        Assert.Equal("invalid_value", nanEqual.Code);

        var badFloatRow = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["f"] = float.PositiveInfinity }
        };
        var infRowErr = Assert.Throws<DashboardException>(() =>
            Filters.Apply(badFloatRow, [Filter.Range("f", 0, 10)]));
        Assert.Equal("invalid_value", infRowErr.Code);

        var badDoubleRow = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["d"] = double.NaN }
        };
        var nanRowErr = Assert.Throws<DashboardException>(() =>
            Filters.Apply(badDoubleRow, [Filter.Range("d", 0, 10)]));
        Assert.Equal("invalid_value", nanRowErr.Code);

        var hugeRows = Enumerable.Repeat<IReadOnlyDictionary<string, object?>>(
            new Dictionary<string, object?> { ["a"] = 1 },
            10_001).ToList();
        var tooManyErr = Assert.Throws<DashboardException>(() =>
            Filters.Apply(hugeRows, [Filter.Equal("a", 1)]));
        Assert.Equal("too_many_points", tooManyErr.Code);
    }

    [Fact]
    public void AcceptanceValidationErrorsLengthMismatchInvalidValueTooManyPointsInvalidIdTooManySeries()
    {
        var mismatchSpec = new ChartSpec(
            "Title",
            "Desc",
            CategoriesJanFeb,
            [new Series("S1", SingleValueOne)],
            ChartKind.Bar);
        var exMismatch = Assert.Throws<DashboardException>(() => ChartRenderer.RenderSvg(mismatchSpec));
        Assert.Equal("length_mismatch", exMismatch.Code);

        var nanSpec = new ChartSpec(
            "Title",
            "Desc",
            SingleCategoryJan,
            [new Series("S1", NanValueArray)],
            ChartKind.Bar);
        var exNan = Assert.Throws<DashboardException>(() => ChartRenderer.RenderSvg(nanSpec));
        Assert.Equal("invalid_value", exNan.Code);

        var infSpec = new ChartSpec(
            "Title",
            "Desc",
            SingleCategoryJan,
            [new Series("S1", NegInfValueArray)],
            ChartKind.Line);
        var exInf = Assert.Throws<DashboardException>(() => ChartRenderer.RenderSvg(infSpec));
        Assert.Equal("invalid_value", exInf.Code);

        var categories10001 = Enumerable.Range(0, 10_001).Select(i => $"C{i}").ToArray();
        var values10001 = Enumerable.Repeat(1.0, 10_001).ToArray();
        var tooManySpec = new ChartSpec(
            "Title",
            "Desc",
            categories10001,
            [new Series("S1", values10001)],
            ChartKind.Bar);
        var exPoints = Assert.Throws<DashboardException>(() => ChartRenderer.RenderSvg(tooManySpec));
        Assert.Equal("too_many_points", exPoints.Code);

        foreach (var badId in BadIdPrefixes)
        {
            var badIdSpec = new ChartSpec(
                "Title",
                "Desc",
                SingleCategoryJan,
                [new Series("S1", SingleValueOne)],
                ChartKind.Bar,
                IdPrefix: badId);
            var exId = Assert.Throws<DashboardException>(() => ChartRenderer.RenderSvg(badIdSpec));
            Assert.Equal("invalid_id", exId.Code);
        }

        var sixSeries = Enumerable.Range(1, 6).Select(i => new Series($"S{i}", [i])).ToArray();
        var tooManySeriesSpec = new ChartSpec(
            "Title",
            "Desc",
            SingleCategoryJan,
            sixSeries,
            ChartKind.Line);
        var exSeries = Assert.Throws<DashboardException>(() => ChartRenderer.RenderSvg(tooManySeriesSpec));
        Assert.Equal("too_many_series", exSeries.Code);

        var shortPaletteTheme = new Theme("#FFFFFF", "#1F2937", BlueSeries);
        var twoSeriesSpec = new ChartSpec(
            "Title",
            "Desc",
            SingleCategoryJan,
            [new Series("S1", SingleValOne), new Series("S2", SingleValTwo)],
            ChartKind.Bar);
        var exShortTheme = Assert.Throws<DashboardException>(() => ChartRenderer.RenderSvg(twoSeriesSpec, shortPaletteTheme));
        Assert.Equal("too_many_series", exShortTheme.Code);

        var emptyCatSpec = new ChartSpec("T", "D", Array.Empty<string>(), [new Series("S", Array.Empty<double>())], ChartKind.Bar);
        Assert.Equal("invalid_value", Assert.Throws<DashboardException>(() => ChartRenderer.RenderSvg(emptyCatSpec)).Code);

        var smallDimSpec = new ChartSpec("T", "D", SingleCategoryA, [new Series("S", SingleValueOne)], ChartKind.Bar, Width: 50, Height: 50);
        Assert.Equal("invalid_value", Assert.Throws<DashboardException>(() => ChartRenderer.RenderSvg(smallDimSpec)).Code);
    }

    [Fact]
    public void AcceptanceDeterministicAndCultureInvariantUnderDeDe()
    {
        var spec = new ChartSpec(
            Title: "Decimal check",
            Description: "Testing de-DE culture invariance",
            Categories: CategoriesQ1Q2,
            Series: [new Series("Growth", GrowthValues)],
            Kind: ChartKind.Line,
            IdPrefix: "growth-chart");

        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var svg1 = ChartRenderer.RenderSvg(spec);
            var svg2 = ChartRenderer.RenderSvg(spec);
            var table = ChartRenderer.RenderTable(spec);

            Assert.Equal(svg1, svg2);
            Assert.Contains("Q1, Growth: 1.5</title>", svg1, StringComparison.Ordinal);
            Assert.DoesNotContain("1,5", svg1, StringComparison.Ordinal);
            Assert.Contains("<td>1.5</td>", table, StringComparison.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void EdgeCasesNegativeValuesSingleCategoryAllEqualValuesMultiSeriesLineDasharray()
    {
        // Negative values in bar chart
        var negSpec = new ChartSpec(
            "Profit and Loss",
            "Includes negative bars",
            ThreeMonths,
            [new Series("Net", ProfitLossValues)],
            ChartKind.Bar);
        var negSvg = ChartRenderer.RenderSvg(negSpec);
        Assert.Contains("<title>Jan, Net: -25</title>", negSvg, StringComparison.Ordinal);

        // All negative values in bar chart
        var allNegSpec = new ChartSpec(
            "Loss only",
            "All negative bars",
            CategoriesJanFeb,
            [new Series("Loss", LossOnlyValues)],
            ChartKind.Bar);
        var allNegSvg = ChartRenderer.RenderSvg(allNegSpec);
        Assert.Contains("<title>Jan, Loss: -10</title>", allNegSvg, StringComparison.Ordinal);

        // Single category and all values equal in a 5-series line chart
        var fiveSeries = Enumerable.Range(1, 5)
            .Select(i => new Series($"Series {i}", FiftyValue))
            .ToArray();
        var singleCatLine = new ChartSpec(
            "Single Point",
            "Five series with equal values on a single category",
            SingleCategoryOnlyCat,
            fiveSeries,
            ChartKind.Line,
            IdPrefix: "line-single");
        var lineSvg = ChartRenderer.RenderSvg(singleCatLine);
        Assert.Equal(5, Regex.Count(lineSvg, "<polyline class=\"series-line\""));
        Assert.Equal(5, Regex.Count(lineSvg, "<circle class=\"point\""));
        Assert.Contains("stroke-dasharray=\"6 3\"", lineSvg, StringComparison.Ordinal);
        Assert.Contains("stroke-dasharray=\"2 2\"", lineSvg, StringComparison.Ordinal);
        Assert.Contains("stroke-dasharray=\"8 3 2 3\"", lineSvg, StringComparison.Ordinal);
        Assert.Contains("stroke-dasharray=\"1 3\"", lineSvg, StringComparison.Ordinal);
    }

    [Fact]
    public void SampleSalesFixtureRendersTwoChartDashboardWithUniqueIdsAndAccessibleStructure()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "fixtures", "sample-sales.json");
        var json = File.ReadAllText(fixturePath);
        using var doc = JsonDocument.Parse(json);

        var rows = new List<IReadOnlyDictionary<string, object?>>();
        foreach (var elem in doc.RootElement.EnumerateArray())
        {
            var dict = new Dictionary<string, object?>
            {
                ["month"] = elem.GetProperty("month").GetString(),
                ["region"] = elem.GetProperty("region").GetString(),
                ["channel"] = elem.GetProperty("channel").GetString(),
                ["amount"] = elem.GetProperty("amount").GetDouble(),
                ["orders"] = elem.GetProperty("orders").GetInt32()
            };
            rows.Add(dict);
        }

        var filtered = Filters.Apply(
            rows,
            [
                Filter.Range("amount", 10, 200),
                Filter.In("region", RegionFilterValues)
            ]);

        Assert.Equal(5, filtered.Count);

        var amountByMonth = ThreeMonths
            .Select(m => filtered
                .Where(r => (string?)r["month"] == m)
                .Sum(r => Convert.ToDouble(r["amount"], CultureInfo.InvariantCulture)))
            .ToArray();
        var ordersByMonth = ThreeMonths
            .Select(m => filtered
                .Where(r => (string?)r["month"] == m)
                .Sum(r => Convert.ToDouble(r["orders"], CultureInfo.InvariantCulture)))
            .ToArray();

        var revenueSpec = new ChartSpec(
            Title: "Regional Revenue",
            Description: "Bar chart of filtered revenue by month",
            Categories: ThreeMonths,
            Series: [new Series("Revenue", amountByMonth)],
            Kind: ChartKind.Bar,
            IdPrefix: "rev-chart");

        var ordersSpec = new ChartSpec(
            Title: "Regional Orders",
            Description: "Line chart of filtered orders by month",
            Categories: ThreeMonths,
            Series: [new Series("Orders", ordersByMonth)],
            Kind: ChartKind.Line,
            IdPrefix: "ord-chart");

        var pageMarkup = ChartRenderer.RenderSvg(revenueSpec) + "\n" + ChartRenderer.RenderSvg(ordersSpec);

        // Verify unique IDs across two charts on the same page
        Assert.Equal(1, Regex.Count(pageMarkup, "id=\"rev-chart-title\""));
        Assert.Equal(1, Regex.Count(pageMarkup, "id=\"rev-chart-desc\""));
        Assert.Equal(1, Regex.Count(pageMarkup, "id=\"ord-chart-title\""));
        Assert.Equal(1, Regex.Count(pageMarkup, "id=\"ord-chart-desc\""));
        Assert.Contains("aria-labelledby=\"rev-chart-title rev-chart-desc\"", pageMarkup, StringComparison.Ordinal);
        Assert.Contains("aria-labelledby=\"ord-chart-title ord-chart-desc\"", pageMarkup, StringComparison.Ordinal);
    }

    [Fact]
    public void DashboardExceptionConstructorsAndTruncationWorkAsExpected()
    {
        var defaultEx = new DashboardException();
        Assert.Equal("invalid_value", defaultEx.Code);

        var msgEx = new DashboardException("short message");
        Assert.Equal("short message", msgEx.Message);

        var inner = new InvalidOperationException("root");
        var wrapped = new DashboardException("wrapped", inner);
        Assert.Same(inner, wrapped.InnerException);

        var longInput = new string('x', 250);
        var truncated = new DashboardException("invalid_value", longInput);
        Assert.Equal(200, truncated.Message.Length);
        Assert.EndsWith("...", truncated.Message, StringComparison.Ordinal);

        var emptyEx = new DashboardException("invalid_value", "");
        Assert.Equal("Dashboard validation error.", emptyEx.Message);
    }
}
