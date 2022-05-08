namespace Zytonomy.Api.Endpoints.External;

public class NoteEndpoints : AuthorizedEndpointBase
{
    private NoteRepository _notes;
    private BlobContainerClient _blobs;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Injection constructor.
    /// </summary>
    /// <param name="identityService">Injected instance of the identity service.</param>
    public NoteEndpoints(
        NoteRepository notes,
        BlobContainerClient blobs,
        AzIdentityService identityService) : base(identityService)
    {
        _notes = notes;
        _blobs = blobs;

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

    // notes/workspace
    [FunctionName(nameof(LoadNotes))]
    public async Task<IActionResult> LoadNotes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "notes/workspace/{workspaceId}")] HttpRequest req,
        string workspaceId,
        ILogger log)
    {
        // TODO: Verify user belongs to workspace

        List<Note> workspaceNotes = await _notes.GetItemsFiltered(
            0, 100,
            n => n.CreatedUtc,
            SortDirection.Descending,
            n => n.WorkspaceId == workspaceId);

        return new OkObjectResult(workspaceNotes);
    }

    [FunctionName(nameof(SaveNote))]
    public async Task<IActionResult> SaveNote(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notes/save")] HttpRequest req,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        // TODO: Verify user belongs to workspace

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        Note note = JsonSerializer.Deserialize<Note>(requestBody, _serializerOptions);

        note.CreatedUtc = System.DateTime.UtcNow.ToString("u");
        note.KbId = RuntimeSettings.KbId;

        // TODO: Make this more elegant
        note.ExtractImages();
        note.ExtractVideos();

        await _notes.UpsertAsync(note);

        await signalRMessages.AddAsync(
            Notify.Workspace(note.WorkspaceId)
                .Of("workspace-note-saved")
                .Message(note)
        );

        return new OkResult();
    }

    [FunctionName(nameof(DeleteNote))]
    public async Task<IActionResult> DeleteNote(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "notes/delete/{noteId}")] HttpRequest req,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        string noteId,
        ILogger log)
    {
        // TODO: Verify user belongs to workspace

        Note note = await _notes.GetByIdAsync(noteId);

        await _notes.Delete(note);

        var deletedNote = new { WorkspaceId = note.WorkspaceId, Id = note.Id };

        await signalRMessages.AddAsync(
            Notify.Workspace(note.WorkspaceId)
                .Of("workspace-note-deleted")
                .Message(deletedNote)
        );

        return new OkResult();
    }


    [FunctionName(nameof(AddComment))]
    public async Task<IActionResult> AddComment(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notes/{noteId}/comments/add")] HttpRequest req,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        string noteId,
        ILogger log)
    {
        // TODO: Verify user belongs to workspace

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        Comment comment = JsonSerializer.Deserialize<Comment>(requestBody, _serializerOptions);
        comment.CreatedUtc = DateTime.UtcNow.ToString("u");

        Note note = await _notes.GetByIdAsync(noteId);

        if(note.Comments == null) {
            note.Comments = new List<Comment>();
        }

        // TODO: This could cause conflict; perform some version check here or synchronize via queue name.
        note.Comments.Insert(0, comment);

        await _notes.UpsertAsync(note);

        await signalRMessages.AddAsync(
            Notify.Workspace(note.WorkspaceId)
                .Of("workspace-note-comment-added")
                .Message(comment) // TODO: Without adding the note ID in the result, we cannot have "reply" comments
        );

        return new OkResult();
    }
}
