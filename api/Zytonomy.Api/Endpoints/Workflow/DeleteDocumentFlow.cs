namespace Zytonomy.Api.Endpoints.Workflow;

/// <summary>
/// Orchestration for deleting a content source from the system.  This causes the following
/// to occur:
///
/// 1. Deletion from the QnA KB using the source name
/// 2. Deletion of the binaries from Azure Blob Storage
/// 3. Removal of the contents from the entities in Cosmos (Workspace)
/// </summary>
public class DeleteDocumentFlow
{
    private QnAMakerClient _qnaClient;
    private CloudStorageAccount _storage;
    private BlobContainerClient _blobContainerClient;
    private WorkspaceRepository _workspaces;
    private readonly JsonSerializerOptions _serializerOptions;

    public DeleteDocumentFlow(
            QnAMakerClient qnaClient,
            BlobContainerClient blobContainerClient,
            WorkspaceRepository workspaces
        )
    {
        _qnaClient = qnaClient;
        _blobContainerClient = blobContainerClient;
        _workspaces = workspaces;

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
    [FunctionName("DeleteDocumentFlow")]
    public async Task Start(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        Workspace workspace = context.GetInput<Workspace>();

        await context.CallActivityAsync<string>("DeleteDocumentFlow_DeleteDocuments", workspace);
        await context.CallActivityAsync<string>("DeleteDocumentFlow_UpdateKb", workspace);

        // Add other activities here
        // TODO: Create audit record?
        // TODO: Other notifications?
    }


    /// <summary>
    /// Workflow activity which acknowledges receipt of the question to the user and workspace.
    /// </summary>
    [FunctionName("DeleteDocumentFlow_DeleteDocuments")]
    public async Task DeleteDocuments(
        [ActivityTrigger] Workspace workspace,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        // Get the source that was deleted.
        ContentSource source = workspace.Sources.FirstOrDefault(s => s.Status == "Deleting");

        if(source == null)
        {
            return;
        }

        string blobName = source.BlobStorageFileName.Replace("zytonomy/", string.Empty); // TODO: Put this elsewhere

        log.LogInformation($">>> Deleting blob name: {blobName}");

        BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

        return;
    }

    /// <summary>
    /// Workflow activity which updates the KB by removing the items based on the source.
    /// </summary>
    [FunctionName("DeleteDocumentFlow_UpdateKb")]
    public async Task<string> UpdateKb(
        [ActivityTrigger] Workspace workspace,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        // Get the source that was deleted.
        ContentSource source = workspace.Sources.FirstOrDefault(s => s.Status == "Deleting");

        if(source == null)
        {
            return null;
        }

        // https://docs.microsoft.com/en-us/rest/api/cognitiveservices-qnamaker/qnamaker5.0preview2/knowledgebase/update#delete
        Operation operation = await _qnaClient
            .Knowledgebase.UpdateAsync(RuntimeSettings.KbId,
                new UpdateKbOperationDTO {
                    Delete = new UpdateKbOperationDTODelete(
                        sources: new [] { source.GetBlobStorageFileNameOnly() }
                    )
                });

        // TODO: Wait?

        // Update the workspace in the repo
        workspace.Sources.RemoveAll(s => s.Status == "Deleting");

        await _workspaces.UpsertAsync(workspace);

        await _qnaClient.Knowledgebase.PublishAsync(RuntimeSettings.KbId); // TODO: Move the ID into a configuration object in Cosmos

        await signalRMessages.AddAsync(
            Notify.Workspace(workspace.Id)
                .Of("workspace-source-deleted")
                .Message(JsonSerializer.Serialize<Workspace>(workspace, _serializerOptions)));

        return operation.OperationId;
    }
}
