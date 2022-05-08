namespace Zytonomy.Api.Endpoints.External;

/// <summary>
/// Endpoints that support messaging functionality.
/// </summary>
public class MessagingEndpoints : AuthorizedEndpointBase
{
    private QnAMakerClient _qnaClient;
    private WorkspaceRepository _workspaces;
    private MessageRepository _messages;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Injection constructor.
    /// </summary>
    /// <param name="identityService">Injected instance of the identity service.</param>
    public MessagingEndpoints(
        MessageRepository messages,
        WorkspaceRepository workspaces,
        QnAMakerClient qnaClient,
        AzIdentityService identityService) : base(identityService)
    {
        _serializerOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };

        _messages = messages;
        _workspaces = workspaces;
        _qnaClient = qnaClient;
    }

    /// <summary>
    /// Asks a question by sending it to the QnA API.  The first leg is to acknowledge the message
    /// and send it back to the group.  The question QnA API is triggered via a queue event to decouple
    /// from synchronous call.
    /// </summary>
    [FunctionName(nameof(AskQuestion))]
    public async Task<IActionResult> AskQuestion(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "messaging/ask")] HttpRequest req,
        [DurableClient] IDurableOrchestrationClient durableWorkflowClient,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        // TODO: Verify that the user belongs to the workspace.
        // TODO: Verify sender matches logged in user.

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        Message message = JsonSerializer.Deserialize<Message>(requestBody, _serializerOptions);

        message.Posted = true;

        // Send an immediate response to the front-end that the question was received and is being processed.
        // This should replace the original message from the user and generate a placeholder for the question
        // to indicate progress on the server.
        await signalRMessages.AddAsync(
            Notify.Workspace(message.WorkspaceId)
                .Of("workspace-question-received")
                .Message(JsonSerializer.Serialize<Message>(message, _serializerOptions)));

        // Start the orchestration for handling question messages
        // There is a lag with this approach based on the use of Storage Queues
        // await durableWorkflowClient.StartNewAsync<Message>(
        //     nameof(QuestionAnswerFlow), message.Id, message);

        Workspace workspace = await _workspaces.GetByIdAsync(message.WorkspaceId);

        // https://docs.microsoft.com/en-us/rest/api/cognitiveservices-qnamaker/qnamaker5.0preview2/knowledgebase/generate-answer
        QueryDTO question = new QueryDTO {
            Top = 3,
            IsTest = false,
            ScoreThreshold = 10,
            StrictFilters = new MetadataDTO[] {
                // Filter the results so we only search for the workspace ID.
                new MetadataDTO {
                    Name = "zyworkspace", // TODO: Move this to a constant.
                    Value = workspace.Id
                }
            }/*,
            AnswerSpanRequest = new QueryDTOAnswerSpanRequest {
                Enable = true,
                ScoreThreshold = 20,
                TopAnswersWithSpan = 1
            }*/
        };

        if(string.IsNullOrEmpty(message.TargetId)) {
            question.Question = message.Body; // Textual question; Text Analytics will parse and give us a response.
        }
        else {
            question.QnaId = message.TargetId; // Direct question; set the direct target we want.
        }

        QnASearchResultList results;

        try {
            results = await _qnaClient.Knowledgebase.GenerateAnswerAsync(RuntimeSettings.KbId, question);
        }
        catch(Exception exception) {
            log.LogError(exception, "Exception on request.");
            throw;
        }

        // Copy the message but update it to reflect the response.
        message.Body = JsonSerializer.Serialize<QnASearchResultList>(results, _serializerOptions);
        message.MessageType = MessageType.BotResponse;
        message.Id = $"placeholder_response_{message.Id}"; // See module-messaging/actions.ts
        message.Author.Id = string.Empty;
        message.Author.Name = "Thinktastic";

        await signalRMessages.AddAsync(
            Notify.Workspace(message.WorkspaceId)
                .Of("qna-question-answered")
                    .Message(message));

        return new OkResult();
    }

    /// <summary>
    /// Sends a chat message to the group.
    /// </summary>
    [FunctionName(nameof(SendMessage))]
    public async Task<IActionResult> SendMessage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "messaging/chat")] HttpRequest req,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        // TODO: Verify that the user belongs to the workspace.
        // TODO: Verify sender matches logged in user.

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        Message message = JsonSerializer.Deserialize<Message>(requestBody, _serializerOptions);

        message.Posted = true;

        await signalRMessages.AddAsync(
            Notify.Workspace(message.WorkspaceId)
                .Of("workspace-chat-received")
                .Message(JsonSerializer.Serialize<Message>(message, _serializerOptions)));

        return new OkResult();
    }
}
