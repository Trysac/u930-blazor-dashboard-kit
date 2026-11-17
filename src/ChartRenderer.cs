using System.Globalization;
using System.Net;
using System.Text;

namespace Trysac.BlazorDashboardKit;

/// <summary>
/// Renders <see cref="ChartSpec"/> definitions as accessible, deterministic SVG markup and fallback HTML data tables.
/// </summary>
public static class ChartRenderer
{
    /// <summary>
    /// Maximum total data points (categories * series) allowed in a chart.
    /// </summary>
    public const int MaxPoints = 10_000;

    /// <summary>
    /// Maximum number of series allowed in a chart.
    /// </summary>
    public const int MaxSeries = 5;

    private const double MarginLeft = 48.0;
    private const double MarginRight = 16.0;
    private const double MarginTop = 32.0;
    private const double MarginBottom = 40.0;

    private static readonly string[] DashPatterns =
    {
        "",
        "6 3",
        "2 2",
        "8 3 2 3",
        "1 3"
    };

    /// <summary>
    /// Renders the specified <paramref name="spec"/> as deterministic, accessible SVG markup.
    /// </summary>
    /// <param name="spec">The chart specification.</param>
    /// <param name="theme">The optional color theme; defaults to <see cref="Theme.Default"/> when null.</param>
    /// <returns>A complete <c>&lt;svg&gt;</c> element string.</returns>
    /// <exception cref="DashboardException">Thrown when validation or WCAG contrast checks fail.</exception>
    public static string RenderSvg(ChartSpec spec, Theme? theme = null)
    {
        ArgumentNullException.ThrowIfNull(spec);
        var activeTheme = theme ?? Theme.Default;

        ValidateSpec(spec);
        ValidateTheme(activeTheme, spec.Series.Count);

        var (dataMin, dataMax) = GetDataBounds(spec);
        var ticks = NiceTicks.Compute(dataMin, dataMax, 5);
        var yMin = ticks[0];
        var yMax = ticks[^1];

        var plotWidth = spec.Width - MarginLeft - MarginRight;
        var plotHeight = spec.Height - MarginTop - MarginBottom;

        double MapY(double value)
        {
            var ratio = (value - yMin) / (yMax - yMin);
            return MarginTop + plotHeight - (ratio * plotHeight);
        }

        var titleId = $"{spec.IdPrefix}-title";
        var descId = $"{spec.IdPrefix}-desc";

        var sb = new StringBuilder(2048);
        sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" role=\"img\" aria-labelledby=\"")
          .Append(titleId).Append(' ').Append(descId)
          .Append("\" width=\"").Append(FormatNumber(spec.Width))
          .Append("\" height=\"").Append(FormatNumber(spec.Height))
          .Append("\" viewBox=\"0 0 ").Append(FormatNumber(spec.Width)).Append(' ').Append(FormatNumber(spec.Height))
          .Append("\">");

        sb.Append("<title id=\"").Append(titleId).Append("\">")
          .Append(Escape(spec.Title))
          .Append("</title>");

        sb.Append("<desc id=\"").Append(descId).Append("\">")
          .Append(Escape(spec.Description))
          .Append("</desc>");

        sb.Append("<rect class=\"chart-bg\" x=\"0\" y=\"0\" width=\"")
          .Append(FormatNumber(spec.Width))
          .Append("\" height=\"")
          .Append(FormatNumber(spec.Height))
          .Append("\" fill=\"")
          .Append(activeTheme.Background)
          .Append("\"/>");

        // Y-axis ticks and gridlines
        sb.Append("<g class=\"y-axis\">");
        foreach (var tick in ticks)
        {
            var y = MapY(tick);
            var yStr = FormatNumber(y);
            var tickLabel = FormatNumber(tick);
            sb.Append("<line class=\"grid-line\" x1=\"")
              .Append(FormatNumber(MarginLeft))
              .Append("\" y1=\"").Append(yStr)
              .Append("\" x2=\"").Append(FormatNumber(spec.Width - MarginRight))
              .Append("\" y2=\"").Append(yStr)
              .Append("\" stroke=\"").Append(activeTheme.Text)
              .Append("\" stroke-opacity=\"0.2\"/>");
            sb.Append("<text class=\"y-tick\" x=\"")
              .Append(FormatNumber(MarginLeft - 6))
              .Append("\" y=\"").Append(yStr)
              .Append("\" fill=\"").Append(activeTheme.Text)
              .Append("\" text-anchor=\"end\" dominant-baseline=\"middle\">")
              .Append(tickLabel)
              .Append("</text>");
        }
        sb.Append("</g>");

        // X-axis category labels
        var categoryCount = spec.Categories.Count;
        var bandWidth = plotWidth / categoryCount;
        sb.Append("<g class=\"x-axis\">");
        for (var c = 0; c < categoryCount; c++)
        {
            var centerX = MarginLeft + ((c + 0.5) * bandWidth);
            var labelY = spec.Height - MarginBottom + 18.0;
            sb.Append("<text class=\"x-tick\" x=\"")
              .Append(FormatNumber(centerX))
              .Append("\" y=\"").Append(FormatNumber(labelY))
              .Append("\" fill=\"").Append(activeTheme.Text)
              .Append("\" text-anchor=\"middle\">")
              .Append(Escape(spec.Categories[c]))
              .Append("</text>");
        }
        sb.Append("</g>");

        // Data series
        if (spec.Kind == ChartKind.Bar)
        {
            RenderBars(sb, spec, activeTheme, bandWidth, yMin, yMax, MapY);
        }
        else
        {
            RenderLines(sb, spec, activeTheme, bandWidth, plotWidth, MapY);
        }

        // Legend (WCAG 1.4.1: text label + color swatch / dash pattern)
        RenderLegend(sb, spec, activeTheme);

        sb.Append("</svg>");
        return sb.ToString();
    }

    /// <summary>
    /// Renders the specified <paramref name="spec"/> as an accessible HTML <c>&lt;table&gt;</c> fallback.
    /// </summary>
    /// <param name="spec">The chart specification.</param>
    /// <returns>An HTML <c>&lt;table&gt;</c> string containing a caption, column headers for each series, and row headers for each category.</returns>
    /// <exception cref="DashboardException">Thrown when the specification is invalid.</exception>
    public static string RenderTable(ChartSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ValidateSpec(spec);

        var sb = new StringBuilder(1024);
        sb.Append("<table><caption>")
          .Append(Escape(spec.Title))
          .Append("</caption><thead><tr><th scope=\"col\">Category</th>");

        foreach (var series in spec.Series)
        {
            sb.Append("<th scope=\"col\">")
              .Append(Escape(series.Name))
              .Append("</th>");
        }

        sb.Append("</tr></thead><tbody>");

        for (var c = 0; c < spec.Categories.Count; c++)
        {
            sb.Append("<tr><th scope=\"row\">")
              .Append(Escape(spec.Categories[c]))
              .Append("</th>");

            for (var s = 0; s < spec.Series.Count; s++)
            {
                var val = spec.Series[s].Values[c];
                sb.Append("<td>")
                  .Append(FormatNumber(val))
                  .Append("</td>");
            }

            sb.Append("</tr>");
        }

        sb.Append("</tbody></table>");
        return sb.ToString();
    }

    private static void RenderBars(
        StringBuilder sb,
        ChartSpec spec,
        Theme theme,
        double bandWidth,
        double yMin,
        double yMax,
        Func<double, double> mapY)
    {
        var seriesCount = spec.Series.Count;
        var groupWidth = bandWidth * 0.8;
        var barWidth = groupWidth / seriesCount;
        var bandOffset = (bandWidth - groupWidth) / 2.0;

        var baselineValue = Math.Clamp(0.0, yMin, yMax);
        var baselineY = mapY(baselineValue);

        sb.Append("<g class=\"series-group\">");
        for (var s = 0; s < seriesCount; s++)
        {
            var series = spec.Series[s];
            var color = theme.SeriesColors[s];
            for (var c = 0; c < spec.Categories.Count; c++)
            {
                var val = series.Values[c];
                var valY = mapY(val);
                var x = MarginLeft + (c * bandWidth) + bandOffset + (s * barWidth);
                var y = Math.Min(valY, baselineY);
                var height = Math.Abs(valY - baselineY);

                var pointTitle = $"{Escape(spec.Categories[c])}, {Escape(series.Name)}: {FormatNumber(val)}";

                sb.Append("<rect class=\"bar\" x=\"")
                  .Append(FormatNumber(x))
                  .Append("\" y=\"").Append(FormatNumber(y))
                  .Append("\" width=\"").Append(FormatNumber(barWidth))
                  .Append("\" height=\"").Append(FormatNumber(height))
                  .Append("\" fill=\"").Append(color)
                  .Append("\"><title>")
                  .Append(pointTitle)
                  .Append("</title></rect>");
            }
        }
        sb.Append("</g>");
    }

    private static void RenderLines(
        StringBuilder sb,
        ChartSpec spec,
        Theme theme,
        double bandWidth,
        double plotWidth,
        Func<double, double> mapY)
    {
        var categoryCount = spec.Categories.Count;
        sb.Append("<g class=\"series-group\">");

        for (var s = 0; s < spec.Series.Count; s++)
        {
            var series = spec.Series[s];
            var color = theme.SeriesColors[s];
            var dash = DashPatterns[s];

            sb.Append("<polyline class=\"series-line\" fill=\"none\" stroke=\"")
              .Append(color)
              .Append("\" stroke-width=\"2\"");

            if (dash.Length > 0)
            {
                sb.Append(" stroke-dasharray=\"").Append(dash).Append('"');
            }

            sb.Append(" points=\"");
            for (var c = 0; c < categoryCount; c++)
            {
                if (c > 0)
                {
                    sb.Append(' ');
                }

                var x = GetLinePointX(c, categoryCount, bandWidth, plotWidth);
                var y = mapY(series.Values[c]);
                sb.Append(FormatNumber(x)).Append(',').Append(FormatNumber(y));
            }
            sb.Append("\"/>");

            for (var c = 0; c < categoryCount; c++)
            {
                var val = series.Values[c];
                var x = GetLinePointX(c, categoryCount, bandWidth, plotWidth);
                var y = mapY(val);
                var pointTitle = $"{Escape(spec.Categories[c])}, {Escape(series.Name)}: {FormatNumber(val)}";

                sb.Append("<circle class=\"point\" cx=\"")
                  .Append(FormatNumber(x))
                  .Append("\" cy=\"").Append(FormatNumber(y))
                  .Append("\" r=\"4\" fill=\"").Append(color)
                  .Append("\"><title>")
                  .Append(pointTitle)
                  .Append("</title></circle>");
            }
        }

        sb.Append("</g>");
    }

    private static double GetLinePointX(int index, int count, double bandWidth, double plotWidth)
    {
        if (count == 1)
        {
            return MarginLeft + (plotWidth / 2.0);
        }

        return MarginLeft + ((index + 0.5) * bandWidth);
    }

    private static void RenderLegend(StringBuilder sb, ChartSpec spec, Theme theme)
    {
        sb.Append("<g class=\"legend\">");
        var currentX = MarginLeft;
        const double legendY = 16.0;

        for (var s = 0; s < spec.Series.Count; s++)
        {
            var series = spec.Series[s];
            var color = theme.SeriesColors[s];
            var dash = DashPatterns[s];

            sb.Append("<g class=\"legend-item\">");
            if (spec.Kind == ChartKind.Line)
            {
                sb.Append("<line x1=\"").Append(FormatNumber(currentX))
                  .Append("\" y1=\"").Append(FormatNumber(legendY))
                  .Append("\" x2=\"").Append(FormatNumber(currentX + 16))
                  .Append("\" y2=\"").Append(FormatNumber(legendY))
                  .Append("\" stroke=\"").Append(color)
                  .Append("\" stroke-width=\"2\"");
                if (dash.Length > 0)
                {
                    sb.Append(" stroke-dasharray=\"").Append(dash).Append('"');
                }
                sb.Append("/>");
            }
            else
            {
                sb.Append("<rect x=\"").Append(FormatNumber(currentX))
                  .Append("\" y=\"").Append(FormatNumber(legendY - 6))
                  .Append("\" width=\"12\" height=\"12\" fill=\"").Append(color)
                  .Append("\"/>");
            }

            var textX = currentX + 20;
            sb.Append("<text x=\"").Append(FormatNumber(textX))
              .Append("\" y=\"").Append(FormatNumber(legendY))
              .Append("\" fill=\"").Append(theme.Text)
              .Append("\" dominant-baseline=\"middle\">")
              .Append(Escape(series.Name))
              .Append("</text></g>");

            currentX += 80.0 + (series.Name.Length * 7.0);
        }

        sb.Append("</g>");
    }

    private static void ValidateSpec(ChartSpec spec)
    {
        if (!IsValidIdPrefix(spec.IdPrefix))
        {
            throw new DashboardException(
                "invalid_id",
                "IdPrefix must match [a-z][a-z0-9-]{0,31}.");
        }

        if (spec.Title is null || spec.Description is null || spec.Categories is null || spec.Series is null)
        {
            throw new DashboardException("invalid_value", "ChartSpec properties must not be null.");
        }

        if (spec.Width < 100 || spec.Height < 100)
        {
            throw new DashboardException("invalid_value", "Chart Width and Height must be at least 100 pixels.");
        }

        if (spec.Categories.Count == 0 || spec.Series.Count == 0)
        {
            throw new DashboardException("invalid_value", "Chart must have at least one category and at least one series.");
        }

        if (spec.Series.Count > MaxSeries)
        {
            throw new DashboardException(
                "too_many_series",
                $"Chart has {spec.Series.Count} series; maximum allowed is {MaxSeries}.");
        }

        var totalPoints = 0L;
        foreach (var series in spec.Series)
        {
            if (series is null || series.Name is null || series.Values is null)
            {
                throw new DashboardException("invalid_value", "Series and its Name and Values must not be null.");
            }

            if (series.Values.Count != spec.Categories.Count)
            {
                throw new DashboardException(
                    "length_mismatch",
                    $"Series '{series.Name}' has {series.Values.Count} values, but there are {spec.Categories.Count} categories.");
            }

            totalPoints += series.Values.Count;
            if (totalPoints > MaxPoints)
            {
                throw new DashboardException(
                    "too_many_points",
                    $"Total data points exceed the limit of {MaxPoints}.");
            }

            for (var i = 0; i < series.Values.Count; i++)
            {
                var v = series.Values[i];
                if (!double.IsFinite(v))
                {
                    throw new DashboardException(
                        "invalid_value",
                        $"Series '{series.Name}' contains a NaN or infinite value at index {i}.");
                }
            }
        }

        foreach (var cat in spec.Categories)
        {
            if (cat is null)
            {
                throw new DashboardException("invalid_value", "Category label must not be null.");
            }
        }
    }

    private static void ValidateTheme(Theme theme, int requiredSeriesCount)
    {
        if (theme.SeriesColors is null || theme.SeriesColors.Count < requiredSeriesCount)
        {
            throw new DashboardException(
                "too_many_series",
                "Theme does not provide enough series colors for the chart.");
        }

        var textRatio = Contrast.Ratio(theme.Text, theme.Background);
        if (textRatio < 4.5)
        {
            throw new DashboardException(
                "low_contrast",
                $"Theme text color {theme.Text} has contrast ratio {FormatNumber(textRatio)} against {theme.Background} (minimum 4.5 required).");
        }

        for (var i = 0; i < requiredSeriesCount; i++)
        {
            var color = theme.SeriesColors[i];
            var seriesRatio = Contrast.Ratio(color, theme.Background);
            if (seriesRatio < 3.0)
            {
                throw new DashboardException(
                    "low_contrast",
                    $"Series color {color} has contrast ratio {FormatNumber(seriesRatio)} against {theme.Background} (minimum 3.0 required).");
            }
        }
    }

    private static (double Min, double Max) GetDataBounds(ChartSpec spec)
    {
        var min = double.PositiveInfinity;
        var max = double.NegativeInfinity;

        foreach (var series in spec.Series)
        {
            foreach (var v in series.Values)
            {
                if (v < min)
                {
                    min = v;
                }

                if (v > max)
                {
                    max = v;
                }
            }
        }

        if (spec.Kind == ChartKind.Bar)
        {
            if (min > 0.0)
            {
                min = 0.0;
            }

            if (max < 0.0)
            {
                max = 0.0;
            }
        }

        return (min, max);
    }

    private static bool IsValidIdPrefix(string? idPrefix)
    {
        if (string.IsNullOrEmpty(idPrefix) || idPrefix.Length > 32)
        {
            return false;
        }

        var first = idPrefix[0];
        if (first is < 'a' or > 'z')
        {
            return false;
        }

        for (var i = 1; i < idPrefix.Length; i++)
        {
            var c = idPrefix[i];
            var valid = (c is >= 'a' and <= 'z') || (c is >= '0' and <= '9') || c == '-';
            if (!valid)
            {
                return false;
            }
        }

        return true;
    }

    private static string FormatNumber(double value)
    {
        if (value == 0.0)
        {
            value = 0.0;
        }

        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string Escape(string text)
    {
        return WebUtility.HtmlEncode(text);
    }
}
