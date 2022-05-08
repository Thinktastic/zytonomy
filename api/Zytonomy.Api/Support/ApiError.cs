namespace Zytonomy.Api.Support;

/// <summary>
/// Class to encapsulate an API error response to the client.
/// </summary>
public class ApiError
{
    /// <summary>
    /// The HTTP code associated with the error.  Defaults  to 500
    /// </summary>
    [JsonIgnore] // TODO: Hmm; this doesn't work on the result because Func is using NSJ.
    public int Code = 500;

    /// <summary>
    /// A message associated with the error.
    /// </summary>
    public string Message;

    /// <summary>
    /// Creates a new API error using the default 500 status code.
    /// </summary>
    /// <param name="message">The message to included with the error response.</param>
    public ApiError(string message) {
        Message = message;
    }
}
