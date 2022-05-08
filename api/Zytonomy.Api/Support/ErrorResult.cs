namespace Zytonomy.Api.Support;

/// <summary>
/// Result class for errors.
/// </summary>
public class ErrorResult : ObjectResult
{
    /// <summary>
    /// An error message associated with this result.
    /// </summary>
    public string Message;

    /// <summary>
    /// Creates an instance of an error result which always sends an error code 500.
    /// </summary>
    /// <param name="message">The message to include with the result.</param>
    /// <returns>The instance of the error result.</returns>
    public ErrorResult(ApiError error) : base(error)
    {
        StatusCode = error.Code;
        Message = error.Message;
    }

    /// <summary>
    /// Convenience method to create an error message with an ApiError with HTTP status 500.
    /// </summary>
    /// <param name="message">The string message to return to the client.</param>
    /// <returns>An instance of the ErrorResult.</returns>
    public static ErrorResult Create(string message) {
        return new ErrorResult(new ApiError(message));
    }
}
