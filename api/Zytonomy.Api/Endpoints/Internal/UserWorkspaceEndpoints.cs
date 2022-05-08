namespace Zytonomy.Api.Endpoints.Internal;

/// <summary>
/// Internal endpoint for managing user and workspaces.
/// </summary>
public class UserWorkspaceEndpoints
{

    private WorkspaceRepository _workspaces;
    private BlobContainerClient _blobContainerClient;
    private UserRepository _users;
    private InvitationRepository _invitations;
    private UserWorkspaceMutator _userWorkspaceMutator;

    /// <summary>
    /// Injection constructor.
    /// </summary>
    public UserWorkspaceEndpoints(
        WorkspaceRepository workspaces,
        UserRepository users,
        InvitationRepository invitations,
        BlobContainerClient blobContainerClient,
        UserWorkspaceMutator userWorkspaceMutator)
    {
        _workspaces = workspaces;
        _users = users;
        _invitations = invitations;
        _blobContainerClient = blobContainerClient;
        _userWorkspaceMutator = userWorkspaceMutator;
    }

    /// <summary>
    /// Reacts to a user being added to a workspace by mutating the user entity to insert the
    /// relation to the workspace and pushes a real-time notification to the user to update the UI.
    /// </summary>
    /// <param name="relation">The relationship to insert.</param>
    [FunctionName(nameof(AddWorkspaceToUser))]
    public void AddWorkspaceToUser(
        [QueueTrigger("user-mutate-add-workspace")] EntityRelation relation,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        _userWorkspaceMutator.ConnectUserAndWorkspace(relation, async (user) => {
            await signalRMessages.AddAsync(
                Notify.User(user.Id)
                    .Of("user-mutate-add-workspace")
                    .Message(relation.EmbeddedEntityRef));
        });
    }
}
