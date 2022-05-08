namespace Zytonomy.Api.Endpoints.Workflow;

/// <summary>
/// Activities used by durable orchestrations to interact with and manage the KB.
/// </summary>
public class KbManagementActivities
{
    private QnAMakerClient _qnaClient;
    private CloudStorageAccount _storage;
    private BlobContainerClient _blobContainerClient;
    private WorkspaceRepository _workspaces;

    public KbManagementActivities(
            QnAMakerClient qnaClient,
            CloudStorageAccount storage,
            BlobContainerClient blobContainerClient,
            WorkspaceRepository workspaces
        )
    {
        _qnaClient = qnaClient;
        _storage = storage;
        _blobContainerClient = blobContainerClient;
        _workspaces = workspaces;
    }

    /// <summary>
    /// Uploads the documents to the KB from Azure Storage Blobs
    /// </summary>
    [FunctionName("KbManagementActivities_UpdateKbSources")]
    public async Task<string> UpdateKbSources([ActivityTrigger] Workspace workspace, ILogger log) {
        log.LogInformation(">>> Creating KB");

        List<FileDTO> files = new List<FileDTO>();

        string baseStorageUri = _storage.BlobEndpoint.AbsoluteUri.TrimEnd('/');

        foreach(ContentSource source in workspace.Sources)
        {
            if(source.Status != "Publishing") // TODO: ENUM? Or other mechanism?
            {
                continue; // Skip documents which are already processed.
            }

            string blobName = source.BlobStorageFileName.Replace("zytonomy/", string.Empty); // TODO: Put this elsewhere

            log.LogInformation($">>> Sending blob name: {blobName}");

            BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);

            string fileUrl = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(10)).AbsoluteUri;

            log.LogInformation($">>> Adding file {fileUrl}");

            files.Add(new FileDTO(
                source.GetBlobStorageFileNameOnly(), // This prevents collisions on file name: {GUID}.pdf
                fileUrl)
            );
        }

        Operation operation = await _qnaClient
            .Knowledgebase.UpdateAsync(RuntimeSettings.KbId,
                new UpdateKbOperationDTO {
                    Add = new UpdateKbOperationDTOAdd { Files = files }
                });

        log.LogInformation($">>> KB created with operation ID: {operation.OperationId}");

        return operation.OperationId;
    }

    /// <summary>
    /// A monitoring activity which tracks the progress of a given operation.
    /// </summary>
    [FunctionName("KbManagementActivities_Monitor")]
    public async Task<string> MonitorKbProvisioning([ActivityTrigger] string operationId, ILogger log) {
        log.LogInformation(">>> Monitoring KB");

        Operation operation = await _qnaClient.Operations.GetDetailsAsync(operationId);

        log.LogInformation($">>> KB Operation has status: {operation.OperationState}");

        if(operation.OperationState == OperationStateType.Failed)
        {
            log.LogError($">>> ERROR: {operation.ErrorResponse.Error.Message}");
            log.LogError($">>> ERROR: {operation.ErrorResponse.Error.Target}");
        }

        return operation.OperationState;
    }

    /// <summary>
    /// To support querying by metadata, we need to update the metadata across each of the
    /// source files that we uploaded since it's not possible to search against the file
    /// name.  Eventually, this should also have roles and other metadata to allow filtering
    /// of the results.
    /// </summary>
    [FunctionName("KbManagementActivities_UpdateMetadata")]
    public async Task<string> UpdateMetadata([ActivityTrigger] Workspace workspace, ILogger log) {
        log.LogInformation(">>> Updating metadata on the KB.");

        List<UpdateQnaDTO> updates = new List<UpdateQnaDTO>();

        foreach(ContentSource source in workspace.Sources)
        {
            string sourceName = source.GetBlobStorageFileNameOnly();

            QnADocumentsDTO documents = await _qnaClient.Knowledgebase.DownloadAsync(
                RuntimeSettings.KbId, EnvironmentType.Test, sourceName);

            foreach(QnADTO qna in documents.QnaDocuments) {
                updates.Add(new UpdateQnaDTO {
                    Id = qna.Id,
                    Metadata = new UpdateQnaDTOMetadata {
                        Add = new List<MetadataDTO> {
                            new MetadataDTO("zysource", sourceName), // Set the source metadata
                            new MetadataDTO("zyworkspace", workspace.Id) // Set the workspace metadata
                        },
                        Delete = new List<MetadataDTO>()
                    }
                });
            }
        }

        Operation operation = await _qnaClient.Knowledgebase.UpdateAsync(RuntimeSettings.KbId, new UpdateKbOperationDTO {
            Update = new UpdateKbOperationDTOUpdate {
                QnaList = updates,
            }
        });

        return operation.OperationId;
    }

    /// <summary>
    /// The publishing activity which initiates the publish action on the knowledge base.
    /// </summary>
    [FunctionName("KbManagementActivities_Publish")]
    public async Task Publish(
        [ActivityTrigger] Workspace workspace,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        log.LogInformation(">>> Publishing");
        

        await _qnaClient.Knowledgebase.PublishAsync(RuntimeSettings.KbId);

        Workspace ws = await _workspaces.GetByIdAsync(workspace.Id);

        bool existingKb = ws.Status == "Published";

        ws.Status = "Published"; // TODO: Change this to some other impl.

        // For each document in the Publishing status in the preserved state, we make an update in the WS
        foreach(ContentSource source in workspace.Sources)
        {
            if(source.Status != "Publishing")
            {
                continue;
            }

            ContentSource matched = ws.Sources.FirstOrDefault(w => w.BlobStorageFileName == source.BlobStorageFileName);

            if(matched != null)
            {
                matched.Status = "Published";
            }
        }

        // Update in the database.
        await _workspaces.UpsertAsync(ws);

        if(existingKb)
        {
            await signalRMessages.AddAsync(
                Notify.Workspace(workspace.Id)
                    .Of("workspace-documents-added")
                    .Message(ws));
        }
        else
        {
            await signalRMessages.AddAsync(
                Notify.User(workspace.CreatedBy.Id)
                    .Of("workspace-documents-provisioned")
                    .Message(ws));
        }
    }
}
