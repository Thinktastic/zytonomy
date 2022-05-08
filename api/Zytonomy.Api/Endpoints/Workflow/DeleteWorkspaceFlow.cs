using Member = Zytonomy.Api.Model.Embedded.Member;
using User = Zytonomy.Api.Model.User;

namespace Zytonomy.Api.Endpoints.Workflow;

/// <summary>
/// Orchestration for deleting a content source from the system.  This causes the following
/// to occur:
///
/// 1. Deletion from the QnA KB using the source name
/// 2. Deletion of the binaries from Azure Blob Storage
/// 3. Removal of the contents from the entities in Cosmos (Workspace)
/// </summary>
public class DeleteWorkspaceFlow
{
    private QnAMakerClient _qnaClient;
    private BlobContainerClient _blobContainerClient;
    private WorkspaceRepository _workspaces;
    private UserRepository _users;
    private NoteRepository _notes;
    private readonly JsonSerializerOptions _serializerOptions;

    public DeleteWorkspaceFlow(
            QnAMakerClient qnaClient,
            BlobContainerClient blobContainerClient,
            WorkspaceRepository workspaces,
            UserRepository users,
            NoteRepository notes
        )
    {
        _qnaClient = qnaClient;
        _blobContainerClient = blobContainerClient;
        _workspaces = workspaces;
        _users = users;
        _notes = notes;

        _serializerOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };
    }

    /// <summary>
    /// Entry point which starts this workflow.
    /// </summary>
    [FunctionName("DeleteWorkspaceFlow")]
    public async Task Start(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        Workspace workspace = context.GetInput<Workspace>();

        log.LogInformation($"> Deleting workspace: {workspace.Name} ({workspace.Id})");

        await context.CallActivityAsync<string>("DeleteWorkspaceFlow_UpdateWorkspaceUsers", workspace);

        log.LogInformation($"> Updated workspace users: {workspace.Name} ({workspace.Id})");

        await context.CallActivityAsync<string>("DeleteWorkspaceFlow_DeleteDocuments", workspace);

        log.LogInformation($"> Removed workspace documents: {workspace.Name} ({workspace.Id})");

        await context.CallActivityAsync<string>("DeleteWorkspaceFlow_DeleteNotes", workspace);

        log.LogInformation($"> Removed workspace notes: {workspace.Name} ({workspace.Id})");

        await context.CallActivityAsync<string>("DeleteWorkspaceFlow_RemoveKbSources", workspace);

        log.LogInformation($"> Removed workspace KB sources and workspace: {workspace.Name} ({workspace.Id})");

        // Add other activities here
        // TODO: Create audit record?
        // TODO: Other notifications?
    }

    /// <summary>
    /// Workflow activity which acknowledges receipt of the question to the user and workspace.
    /// </summary>
    [FunctionName("DeleteWorkspaceFlow_DeleteDocuments")]
    public async Task DeleteDocuments(
        [ActivityTrigger] Workspace workspace,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        // Delete each source item from blob storage.
        foreach(ContentSource source in workspace.Sources)
        {
            string blobName = source.BlobStorageFileName.Replace("zytonomy/", string.Empty); // TODO: Put this elsewhere

            log.LogInformation($">>> Deleting blob name: {blobName}");

            BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        // TODO: Delete the folder in storage, too?
    }

    /// <summary>
    /// Workflow activity which acknowledges receipt of the question to the user and workspace.
    /// </summary>
    [FunctionName("DeleteWorkspaceFlow_DeleteNotes")]
    public async Task DeleteNotes(
        [ActivityTrigger] Workspace workspace,
        ILogger log)
    {
        // Delete the notes in the workspace using bulk operations
        // See: https://docs.microsoft.com/en-us/azure/cosmos-db/tutorial-sql-api-dotnet-bulk-import
        // Get the list of notes and then bulk delete.
        List<Note> notes = await _notes.GetItemsFiltered(0, 500,
            n => n.CreatedUtc, SortDirection.Ascending,
            n => n.WorkspaceId == workspace.Id);

        List<Task> tasks = new List<Task>(notes.Count);

        foreach(Note note in notes)
        {
            tasks.Add(_notes.Delete(note));
        }

        await Task.WhenAll(tasks);

        log.LogInformation($">> Removed {notes.Count} notes from workspace {workspace.Name}");
    }

    /// <summary>
    /// Workflow activity which updates the KB by removing the items based on the source.
    /// </summary>
    [FunctionName("DeleteWorkspaceFlow_RemoveKbSources")]
    public async Task RemoveKbSources(
        [ActivityTrigger] Workspace workspace,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        // Get all sources; we don't delete the KB; just the sources associated with this workspace.
        List<string> sources = workspace.Sources
            .Select(s => s.GetBlobStorageFileNameOnly())
            .ToList();

        foreach(string source in sources)
        {
            log.LogInformation($">> Deleting KB source: {source}");
        }

        // https://docs.microsoft.com/en-us/rest/api/cognitiveservices-qnamaker/qnamaker5.0preview2/knowledgebase/update#delete
        Operation operation = await _qnaClient
            .Knowledgebase.UpdateAsync(RuntimeSettings.KbId,
                new UpdateKbOperationDTO {
                    Delete = new UpdateKbOperationDTODelete(
                        sources: sources
                    )
                });

        await _qnaClient.Knowledgebase.PublishAsync(RuntimeSettings.KbId);

        await _workspaces.Delete(workspace);

        await signalRMessages.AddAsync(
            Notify.Workspace(workspace.Id)
                .Of("workspace-deleted")
                .Message(JsonSerializer.Serialize<Workspace>(workspace, _serializerOptions)));
    }

    // TODO: This has to be moved into a queue to reduce spikes in RU for large use cases.
    /// <summary>
    /// Workflow activity which updates worskpace members and removes their connections
    /// </summary>
    [FunctionName("DeleteWorkspaceFlow_UpdateWorkspaceUsers")]
    public async Task UpdateWorkspaceUsers(
        [ActivityTrigger] Workspace workspace,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        // TODO: Batch lookup and batch update.
        foreach(Member member in workspace.Members)
        {
            User user = await _users.GetByIdAsync(member.User.Id);

            user.Workspaces.RemoveAll(w => w.Id == workspace.Id);

            await _users.UpsertAsync(user);

            await signalRMessages.AddAsync(
                Notify
                    .User(user.Id)
                    .Of("workspace-deleted-for-user")
                    .Message(workspace.Id));

            log.LogInformation($">> Removed workspace {workspace.Name} from user {user.Name}");
        }
    }
}
