namespace Zytonomy.Api.Endpoints.External;

/// <summary>
/// Endpoints that support SignalR initialization and connection management as well as
/// Azure Communication Service.
/// </summary>
public class RealTimeEndpoints : AuthorizedEndpointBase
{
    /// <summary>
    /// Injection constructor.
    /// </summary>
    /// <param name="identityService">Injected instance of the identity service.</param>
    public RealTimeEndpoints(AzIdentityService identityService) : base(identityService)
    {
    }

    /// <summary>
    /// Negotiates the SignalR connection using Azure Functions imperative bindings.  This is key for
    /// establishing the client side identity of the user for the client scripts.
    /// </summary>
    [FunctionName("negotiate")]
    public async Task<SignalRConnectionInfo> InitializeSignalRConnection(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        IBinder binder)
    {
        SignalRConnectionInfoAttribute attribute = new SignalRConnectionInfoAttribute
        {
            HubName = $"users_{Environment.GetEnvironmentVariable("Env_Suffix")}",
            UserId = Identity.Id,
            ConnectionStringSetting = "AzureSignalRConnectionString"
        };

        SignalRConnectionInfo connection = await binder.BindAsync<SignalRConnectionInfo>(attribute);

        return connection;
    }

    /// <summary>
    /// Joins a user to a group based on the workspace ID>
    /// </summary>
    [FunctionName(nameof(JoinGroup))]
    public async Task<IActionResult> JoinGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "realtime/join/{workspaceId}")]HttpRequest req,
        string workspaceId,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRGroupAction> signalRGroupActions)
    {
        await signalRGroupActions.AddAsync(
            new SignalRGroupAction
            {
                UserId = Identity.Id,
                GroupName = workspaceId,
                Action = GroupAction.Add
            });

        return new OkResult();
    }
}
