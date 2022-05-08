using User = Zytonomy.Api.Model.User;

namespace Zytonomy.Api.Endpoints.External;

/// <summary>
/// Endpoint for content related operations.
/// </summary>
public class WorkspaceEndpoints : AuthorizedEndpointBase
{
    private WorkspaceRepository _workspaces;
    private BlobContainerClient _blobContainerClient;
    private UserRepository _users;
    private InvitationRepository _invitations;
    private UserWorkspaceMutator _userWorkspaceMutator;

    /// <summary>
    /// Injection constructor.
    /// </summary>
    public WorkspaceEndpoints(
        WorkspaceRepository workspaces,
        UserRepository users,
        InvitationRepository invitations,
        BlobContainerClient blobContainerClient,
        UserWorkspaceMutator userWorkspaceMutator,
        AzIdentityService identity) : base(identity) {
        _workspaces = workspaces;
        _users = users;
        _invitations = invitations;
        _blobContainerClient = blobContainerClient;
        _userWorkspaceMutator = userWorkspaceMutator;
    }

    /// <summary>
    /// Retrieves a workspace by the supplied ID>
    /// </summary>
    [FunctionName(nameof(GetWorkspace))]
    public async Task<IActionResult> GetWorkspace(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workspace/{id}")] HttpRequest req,
        string id,
        ILogger log)
    {
        Workspace workspace = await _workspaces.GetByIdAsync(id);

        User user = await _users.GetByIdAsync(Identity.Id);

        if(!user.Workspaces.Exists(w => w.Id == id)) {
            return new UnauthorizedObjectResult("You do not have access to this workspace.");
        }

        return new OkObjectResult(workspace);
    }

    /// <summary>
    /// Gets the secure content URL for a content item at a given index in the workspace content sources.
    /// </summary>
    [FunctionName(nameof(GetSecureContentUrl))]
    public async Task<IActionResult> GetSecureContentUrl(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workspace/{workspaceId}/content/{index}/secure")] HttpRequest req,
        string workspaceId,
        int index,
        ILogger log)
    {
        // TODO: Add auth check.

        Workspace workspace = await _workspaces.GetByIdAsync(workspaceId);

        ContentSource source = workspace.Sources[index]; // TODO: Add error checks for missing content.

        string blobName = source.BlobStorageFileName.Replace("zytonomy/", string.Empty);

        BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);

        // Perhaps better to add longer interval and cache for the client.
        string fileUrl = blobClient.GenerateSasUri(
            BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(5)).AbsoluteUri;

        return new OkObjectResult(fileUrl);
    }

    /// <summary>
    /// Creates a new workspace which includes uploaded files.  Body contains a multi-part
    /// POST request which includes the fields title, description, and one or more files.
    /// </summary>
    [FunctionName(nameof(CreateWorkspace))]
    public async Task<IActionResult> CreateWorkspace(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workspace/create")] HttpRequest req,
        [Queue("user-mutate-add-workspace")] IAsyncCollector<EntityRelation> workspaceCollector,
        [DurableClient] IDurableOrchestrationClient durableWorkflowClient,
        IBinder blobBinder,
        ILogger log)
    {
        // Create the content set.
        Workspace workspace = new Workspace {
            Name = req.Form["title"],
            Description = req.Form["description"],
            Id = Guid.NewGuid().ToString(),
            CreatedBy = Identity.GenericRef,
            Sources = new List<ContentSource>(),
            Status = "Provisioning" // TODO: Change this to some other impl.
        };

        DateTime now = DateTime.UtcNow;

        // Upload the files.
        workspace.AddFiles(Identity.Id, blobBinder, req.Form.Files);

        await _workspaces.UpsertAsync(workspace);

        // Add the workspace to the user.
        await workspaceCollector.AddAsync(Identity.GenericRef.Embed(workspace));

        // Initiate a durable orchestration.  Use the workspace ID as the instance ID
        log.LogInformation("Starting durable orchestration for workspace provisioning...");

        await durableWorkflowClient.StartNewAsync<Workspace>(
            "WorkspaceProvisioningFlow", workspace.Id, workspace);

        log.LogInformation("Started durable workflow.");

        return new OkObjectResult(workspace.Id);
    }

    /// <summary>
    /// Adds source file to an existing workspace.  Body contains a multi-part
    /// POST request which includes one or more files.
    /// </summary>
    [FunctionName(nameof(AddSourceFilesToWorkspace))]
    public async Task<IActionResult> AddSourceFilesToWorkspace(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workspace/{workspaceId}/files/add")] HttpRequest req,
        [DurableClient] IDurableOrchestrationClient durableWorkflowClient,
        IBinder blobBinder,
        string workspaceId,
        ILogger log)
    {
        Workspace workspace = await _workspaces.GetByIdAsync(workspaceId);

        // Add the documents to blob storage
        workspace.AddFiles(Identity.Id, blobBinder, req.Form.Files);

        // Update the workspace source list with the documents
        await _workspaces.UpsertAsync(workspace);

        // Trigger the durable client to start a new ingest and publishing phase.
        await durableWorkflowClient.StartNewAsync<Workspace>(
            "WorkspaceProvisioningFlow", $"{workspace.Id}_{Guid.NewGuid()}", workspace);

        // Trigger SignalR update to clients that files were added

        return new OkResult();
    }

    /// <summary>
    /// Removes a source from the workspace.
    /// </summary>
    [FunctionName(nameof(RemoveSourceFromWorkspace))]
    public async Task<IActionResult> RemoveSourceFromWorkspace(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "workspace/{workspaceId}/sources/remove/{index}")] HttpRequest req,
        [DurableClient] IDurableOrchestrationClient durableWorkflowClient,
        string workspaceId,
        int index,
        ILogger log)
    {
        Workspace workspace = await _workspaces.GetByIdAsync(workspaceId);

        workspace.Sources[index].Status = "Deleting"; // Mark as deleting.

        // Trigger the durable client to start a new ingest and publishing phase.
        await durableWorkflowClient.StartNewAsync<Workspace>(
            "DeleteDocumentFlow", $"{workspace.Id}_{Guid.NewGuid()}", workspace);

        return new OkResult();
    }

    /// <summary>
    /// Deletes a workspace
    /// </summary>
    [FunctionName(nameof(DeleteWorkspace))]
    public async Task<IActionResult> DeleteWorkspace(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "workspace/{workspaceId}")] HttpRequest req,
        [DurableClient] IDurableOrchestrationClient durableWorkflowClient,
        string workspaceId,
        ILogger log)
    {
        Workspace workspace = await _workspaces.GetByIdAsync(workspaceId);

        if(Identity.Id != workspace.CreatedBy.Id)
        {
            return new UnauthorizedResult();
        }

        // Trigger the durable client to start a new ingest and publishing phase.
        await durableWorkflowClient.StartNewAsync<Workspace>(
            "DeleteWorkspaceFlow", $"{workspace.Id}_{Guid.NewGuid()}", workspace);

        return new OkResult();
    }

    /// <summary>
    /// Generates a user invite via SendGrid to the current workspace.  The process also creates a pending invitation.
    /// </summary>
    [FunctionName(nameof(InviteUserToWorkspaceAsync))]
    public async Task<IActionResult> InviteUserToWorkspaceAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workspace/{workspaceId}/invite")] HttpRequest req,
        [SendGrid(ApiKey = "SendGrid_ApiKey")] IAsyncCollector<SendGridMessage> messages,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        string workspaceId,
        ILogger log
    )
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        Invitation invitation = JsonSerializer.Deserialize<Invitation>(
            requestBody,
            new JsonSerializerOptions{
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            });

        Workspace workspace = await _workspaces.GetByIdAsync(workspaceId);

        string url = Environment.GetEnvironmentVariable("Env_Url");

        SendGridMessage message = new SendGridMessage();
        message.AddTo(invitation.Email);
        message.AddContent("text/html", $@"
        <p><b>Hi {invitation.FirstName}</b></p>
        <p>You've been invited to collaborate in the workspace <b>{workspace.Name}</b> by <b>{Identity.FirstName} {Identity.LastName}</b>.</p>
        <blockquote>{invitation.Message}</blockquote>
        <p>Have questions?  Reach out to <b>{Identity.FirstName}</b> (<a href='mailto:{Identity.Email}'>{Identity.Email}</a>)</b></p>
        <p><a href='{url}'>Click here to go to {url} and create your account.</a></p>");
        message.SetFrom(new EmailAddress("info@thinktastic.com", "Thinktastic Team"));
        message.SetSubject($"{Identity.FirstName} has invited you to collaborate on: {workspace.Name}");

        await messages.AddAsync(message);

        // Add the user to pending invitations.
        invitation.Id = Guid.NewGuid().ToString("N");
        invitation.WorkspaceId = workspaceId;
        invitation.InvitedBy = Identity.GenericRef;
        invitation.CreatedUtc = DateTime.UtcNow.ToString("u");
        invitation.Status = "Pending"; // TODO: Make enums?
        invitation.Name = workspace.Name;

        await _invitations.UpsertAsync(invitation);

        // If the user exists, let's send a SignalR notification as well.
        User user = await _users.GetItemFiltered(u => u.Email == invitation.Email);

        if(user != null)
        {
            await signalRMessages.AddAsync(
                Notify.User(user.Id)
                    .Of("user-invited-to-workspace")
                    .Message(invitation)
            );
        }

        return new OkObjectResult(invitation);
    }

    /// <summary>
    /// Gets the workspace invitations for a given user using the email address.
    /// </summary>
    [FunctionName(nameof(GetUserWorkspaceInvitations))]
    public async Task<IActionResult> GetUserWorkspaceInvitations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/invitations/pending")] HttpRequest req,
        ILogger log)
    {
        List<Invitation> invitations = await _invitations.GetItemsFiltered(
            0, 100, i => i.CreatedUtc,
            DataAccess.Support.SortDirection.Ascending,
            i => i.Email == Identity.Email,
            i => i.Status == "Pending");

        return new OkObjectResult(invitations);
    }

    /// <summary>
    /// Get the set of invitations for a given workspace
    /// </summary>
    [FunctionName(nameof(GetWorkspaceInvitations))]
    public async Task<IActionResult> GetWorkspaceInvitations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workspace/{workspaceId}/invitations/{status}")] HttpRequest req,
        string workspaceId,
        string status,
        ILogger log)
    {
        List<Invitation> invitations = await _invitations.GetItemsFiltered(
            0, 100, i => i.CreatedUtc,
            DataAccess.Support.SortDirection.Ascending,
            i => i.WorkspaceId == workspaceId,
            i => i.Status == status);

        return new OkObjectResult(invitations);
    }

    /// <summary>
    /// Updates the status of the invitation and adds the user to the workspace.
    /// </summary>
    [FunctionName(nameof(AcceptWorkspaceInvitation))]
    public async Task<IActionResult> AcceptWorkspaceInvitation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/invitations/{invitationId}/accept")] HttpRequest req,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        string invitationId,
        ILogger log)
    {
        Invitation invitation = await _invitations.GetByIdAsync(invitationId);

        if(Identity.Email != invitation.Email) {
            return new UnauthorizedObjectResult("The invitation does not match your account information.");
        }

        // Update the status of the invitation
        invitation.Status = "Accepted"; // TODO: Consider moving this to an enum or other type of set.
        await _invitations.UpsertAsync(invitation);

        Workspace workspace = await _workspaces.GetByIdAsync(invitation.WorkspaceId);

        EntityRelation relation = Identity.GenericRef.Embed(workspace);

        // Trigger an update to add the user to the workspace and the workspace to the user.
        _userWorkspaceMutator.ConnectUserAndWorkspace(relation, async (user) => {
            await signalRMessages.AddAsync(
                Notify.User(user.Id)
                    .Of("user-accept-invitation")
                    .Message(relation.EmbeddedEntityRef));
        });

        return new OkResult();
    }
}
