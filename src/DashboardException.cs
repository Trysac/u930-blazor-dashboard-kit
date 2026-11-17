namespace Trysac.BlazorDashboardKit;

/// <summary>
/// Exception thrown when chart specifications, themes, ticks, or filters violate validation or accessibility rules.
/// </summary>
public sealed class DashboardException : ArgumentException
{
    /// <summary>
    /// Gets the stable machine-readable error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardException"/> class with adefault code and message.
    /// </summary>
    public DashboardException()
        : base("Dashboard validation error.")
    {
        Code = "invalid_value";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DashboardException(string message)
        : base(Truncate(message))
    {
        Code = "invalid_value";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DashboardException(string message, Exception innerException)
        : base(Truncate(message), innerException)
    {
        Code = "invalid_value";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardException"/> class with a machine-readable code and message.
    /// </summary>
    /// <param name="code">The stable error code.</param>
    /// <param name="message">The human-readable error message.</param>
    public DashboardException(string code, string message)
        : base(Truncate(message))
    {
        ArgumentNullException.ThrowIfNull(code);
        Code = code;
    }

    private static string Truncate(string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return "Dashboard validation error.";
        }

        return message.Length <= 200 ? message : string.Concat(message.AsSpan(0, 197), "...");
    }
}
