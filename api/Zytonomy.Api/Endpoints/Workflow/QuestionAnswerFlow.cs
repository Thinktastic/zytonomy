namespace Zytonomy.Api.Endpoints.Workflow;

/// <summary>
/// This orchestration manages the question and answer flow to allow for extension of the
/// logic around the question and answer functionality.
/// </summary>
public class QuestionAnswerFlow
{
    private QnAMakerClient _qnaClient;
    private WorkspaceRepository _workspaces;
    private readonly JsonSerializerOptions _serializerOptions;

    public QuestionAnswerFlow(QnAMakerClient qnaClient, WorkspaceRepository workspaces) {
        _qnaClient = qnaClient;
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
    [FunctionName("QuestionAnswerFlow")]
    public async Task Start(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        Message message = context.GetInput<Message>();

        await context.CallActivityAsync<string>("QuestionAnswerFlow_SubmitQuestion", message);

        // Add other activities here
        // TODO: Persist message?
        // TODO: Compile analytics on common questions/topics?
    }

    /// <summary>
    /// Workflow activity which submits the question to the QnA API
    /// </summary>
    [FunctionName("QuestionAnswerFlow_SubmitQuestion")]
    public async Task SubmitQuestion(
        [ActivityTrigger] Message message,
        [SignalR(HubName = "users_%Env_Suffix%")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
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
    }
}
