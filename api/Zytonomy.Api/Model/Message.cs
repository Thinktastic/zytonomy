namespace Zytonomy.Api.Model;

/// <summary>
/// Represents a message which can be a question, chat, or response.
/// </summary>
[Container("Core")]
public class Message : DocumentEntityBase
{
    public override string PartitionKey => Id;

    /// <summary>
    /// The ID of the workspace that the message is associated with.
    /// </summary>
    public string WorkspaceId;

    /// <summary>
    /// A string value which indicates the date and time that the message
    /// was created.
    /// </summary>
    public string CreatedUtc;

    /// <summary>
    /// The ID of the parent message, if present, for this message.  This is used
    /// for example when the mesage is a response to another message.
    /// </summary>
    public string ParentMessageId;

    /// <summary>
    /// A title or name associated with the message.
    /// </summary>
    [JsonPropertyName("title")]
    public override string Name { get => base.Name; set => base.Name = value; }

    /// <summary>
    /// The body of the message.
    /// </summary>
    public string Body;

    /// <summary>
    /// When non-zero, this is the ID of a direct QnA item to retrieve.
    /// </summary>
    public string TargetId;

    /// <summary>
    /// A reference to the author.
    /// </summary>
    public GenericRef Author;

    /// <summary>
    /// When set to true, this indicates that the message is from the
    /// server.  This value is initially false before the message is
    /// saved.
    /// </summary>
    public bool Posted;

    /// <summary>
    /// The message type or message context that this message represents.
    /// </summary>
    public MessageType MessageType;
}

/// <summary>
/// Enum which represents the intent of the message.
/// </summary>
public enum MessageType {
    // A user chat message
    UserChat,
    // A user question message
    UserQuestion,
    // A direct reference to a QnA item; in this case, usually a followup prompt.
    UserQuestionDirect,
    // A message representing a placeholder for a bot message
    BotPlaceholder,
    // A bot response mesage
    BotResponse,
    // A message that has been saved as a ntoe.
    SavedNote,
    // A message that represents a user created note.
    UserCreatedNote,
    // A message that represents a comment on a saved or user created note.
    NoteComment,
    // A message indicating a meeting request.
    MeetingRequest,
    // A message indiciating the end of a meeting.
    EndMeeting
}
