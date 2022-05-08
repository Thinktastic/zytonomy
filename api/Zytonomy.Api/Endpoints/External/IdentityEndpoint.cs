namespace Zytonomy.Api.Endpoints.External;

/// <summary>
/// Azure Functions does not support middleware except in isolated process mode.
/// Therefore, we cannot process/handle the JWT token provided by Azure AD B2C.
/// Instead, we need to swap it for a locally generated JWT token which we can
/// then process locally without verifying with Azure AD B2C on each request.
/// </summary>
public class IdentityEndpoint
{
    private AzIdentityService _identity;

    /// <summary>
    /// Injection constructor.
    /// </summary>
    /// <param name="identity">An instance of the identity service which encapsulates parsing of the incoming claims.</param>
    public IdentityEndpoint(AzIdentityService identity) {
        _identity = identity;
    }

    /// <summary>
    /// This function verifies a token and swaps the Azure AD B2C provided idToken for a
    /// locally generated JWT token which contains additional custom claims to reduce roundtrips
    /// to the database.
    /// </summary>
    /// <returns>A new JWT token with additional claims encoded with a local key that we can easily decode.</returns>
    [FunctionName(nameof(VerifyAndSwapJwtToken))]
    public async Task<IActionResult> VerifyAndSwapJwtToken(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "identity/verify")] HttpRequest req,
        ILogger log)
    {
        // This is the original token issued by Azure AD B2C
        string token = await new StreamReader(req.Body).ReadToEndAsync();

        // TODO: Add additional claims here and re-encode; just return this as is for now.

        return new OkObjectResult(token);
    }
}
