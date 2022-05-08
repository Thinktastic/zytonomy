using Identity = Zytonomy.Api.Model.Identity;

namespace Zytonomy.Api.Endpoints.External;

/// <summary>
/// Base class for services which require access to the claims principal
/// </summary>
public abstract class AuthorizedEndpointBase : IFunctionInvocationFilter
{
    private const string AuthenticationHeaderName = "Authorization";

    private AzIdentityService _identityService;

    /// <summary>
    /// Inheriting classes should implement this as an injection constructor.
    /// </summary>
    /// <param name="identityService">An instance of the identity service.</param>
    protected AuthorizedEndpointBase(AzIdentityService identityService) {
        _identityService = identityService;
    }

    /// <summary>
    /// The identity of the current user as extracted from the claims in the incoming idToken
    /// </summary>
    protected Identity Identity { get; private set; }

    /// <summary>
    ///     Pre-execution filter.
    /// </summary>
    /// <remarks>
    ///     This mechanism can be used to extract the authentication information.
    /// </remarks>
    public Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
    {
        HttpRequest request = executingContext.Arguments.First().Value as HttpRequest;

        if (request == null || !request.Headers.ContainsKey(AuthenticationHeaderName))
        {
            return Task.FromException(new AuthenticationException("No Authorization header was present"));
        }

        try
        {
            string authorizationHeader = request.Headers[AuthenticationHeaderName];

            if (authorizationHeader.StartsWith("Bearer"))
            {
                authorizationHeader = authorizationHeader.Substring(7);
            }

            ClaimsPrincipal claimsPrincipal = _identityService.GetClaimsPrincipal(authorizationHeader);

            Identity = new Identity(claimsPrincipal);
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }

        if (!Identity.IsValid)
        {
            return Task.FromException(new KeyNotFoundException("No identity key was found in the claims."));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Post-execution filter.
    /// </summary>
    public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
    {
        // Nothing.
        return Task.CompletedTask;
    }
}
