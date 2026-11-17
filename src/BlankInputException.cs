namespace Trysac.BlazorDashboardKit;

/// <summary>Thrown when a text input is empty or whitespace only.</summary>
public sealed class BlankInputException : ArgumentException
{
    /// <summary>Creates the exception with the default message.</summary>
    public BlankInputException() : base("input is blank")
    {
    }

    /// <summary>Creates the exception with a custom message.</summary>
    /// <param name="message">The error message.</param>
    public BlankInputException(string message) : base(message)
    {
    }

    /// <summary>Creates the exception with a custom message and inner cause.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The underlying cause.</param>
    public BlankInputException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
