namespace Trysac.BlazorDashboardKit;

/// <summary>
/// Computes WCAG 2.2 relative luminance and contrast ratios between <c>#RRGGBB</c> colors.
/// </summary>
public static class Contrast
{
    /// <summary>
    /// Computes the WCAG 2.2 contrast ratio between two <c>#RRGGBB</c> hexadecimal color strings.
    /// </summary>
    /// <param name="foreground">The foreground color in <c>#RRGGBB</c> format.</param>
    /// <param name="background">The background color in <c>#RRGGBB</c> format.</param>
    /// <returns>A contrast ratio between 1.0 and 21.0.</returns>
    /// <exception cref="DashboardException">Thrown with code <c>invalid_color</c> if either color is not <c>#RRGGBB</c>.</exception>
    public static double Ratio(string foreground, string background)
    {
        var l1 = RelativeLuminance(foreground);
        var l2 = RelativeLuminance(background);
        var lighter = Math.Max(l1, l2);
        var darker = Math.Min(l1, l2);
        return (lighter + 0.05) / (darker + 0.05);
    }

    internal static double RelativeLuminance(string hexColor)
    {
        if (hexColor is null || hexColor.Length != 7 || hexColor[0] != '#')
        {
            throw new DashboardException("invalid_color", "Color must be in #RRGGBB format.");
        }

        var r = ParseHexByte(hexColor[1], hexColor[2]);
        var g = ParseHexByte(hexColor[3], hexColor[4]);
        var b = ParseHexByte(hexColor[5], hexColor[6]);

        var rLin = Linearize(r / 255.0);
        var gLin = Linearize(g / 255.0);
        var bLin = Linearize(b / 255.0);

        return (0.2126 * rLin) + (0.7152 * gLin) + (0.0722 * bLin);
    }

    private static double Linearize(double channel)
    {
        return channel <= 0.04045
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }

    private static int ParseHexByte(char high, char low)
    {
        var h = HexVal(high);
        var l = HexVal(low);
        if (h < 0 || l < 0)
        {
            throw new DashboardException("invalid_color", "Color must contain only hexadecimal digits in #RRGGBB format.");
        }

        return (h << 4) | l;
    }

    private static int HexVal(char c)
    {
        if (c is >= '0' and <= '9')
        {
            return c - '0';
        }

        if (c is >= 'A' and <= 'F')
        {
            return c - 'A' + 10;
        }

        if (c is >= 'a' and <= 'f')
        {
            return c - 'a' + 10;
        }

        return -1;
    }
}
