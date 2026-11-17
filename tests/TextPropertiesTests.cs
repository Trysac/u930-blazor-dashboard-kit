using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace Trysac.BlazorDashboardKit.Tests;

/// <summary>Property-based tests of <see cref="Text"/>.</summary>
public class TextPropertiesTests
{
    [Property]
    public Property NormalizeIsIdempotentOnNonBlankText(NonEmptyString raw)
    {
        var trimmed = raw.Get.Trim();
        if (trimmed.Length == 0)
        {
            return true.ToProperty();
        }
        var once = Text.Normalize(raw.Get);
        var twice = Text.Normalize(once);
        return (once == twice && once == once.Trim() && string.Equals(once, once.ToLowerInvariant(), StringComparison.Ordinal)).ToProperty();
    }
}
