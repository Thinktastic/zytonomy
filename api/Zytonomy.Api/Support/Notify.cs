namespace Zytonomy.Api.Support;

/// <summary>
/// Convenience mechanism for generating notifications.
/// </summary>
public static class Notify
{
    /// <summary>
    /// Initiate a user targeted notification.  Chain the call to Of() to specify the event.
    /// </summary>
    /// <param name="userId">The ID of the user to notify.</param>
    /// <returns>A user notification instance.  Call Of() next to specify the event.</returns>
    public static UserNotification User(string userId)
    {
        return new UserNotification(userId);
    }

    /// <summary>
    /// Initiate a workspace targeted notification.  Chain the call to Of() to specify the event.
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace to notify.</param>
    /// <returns>A workspace notification instance.  Call Of() next to specify the event.</returns>
    public static WorkspaceNotification Workspace(string workspaceId) {
        return new WorkspaceNotification(workspaceId);
    }
}

public abstract class NotificationBase {
    public virtual string Event { get ; protected set; }

    /// <summary>
    /// Chain with a call to Send()
    /// </summary>
    public virtual NotificationBase Of(string @event) {
        Event = @event;

        return this;
    }

    /// <summary>
    /// The body of the mesage to send.  Can be empty.
    /// </summary>
    public virtual  SignalRMessage Message(params object[] messages) {
        JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        for (int i = 0; i < messages.Length; i++)
        {
            if (messages[i].GetType().IsPrimitive || messages[i] is string)
            {
                continue;  // NEXT: Don't convert string or primitives.
            }

            messages[i] = JsonSerializer.Serialize(messages[i], serializerOptions);
        }

        return BuildMessage(messages);
    }

    protected abstract SignalRMessage BuildMessage(object[] arguments);
}

/// <summary>
/// Encapsulates a workspace notification.
/// </summary>
public class WorkspaceNotification : NotificationBase {
    public string WorkspaceId { get; private set; }

    public WorkspaceNotification(string workspaceId) {
        WorkspaceId = workspaceId;
    }
    protected override SignalRMessage BuildMessage(object[] arguments)
    {
        return new SignalRMessage
            {
                GroupName = WorkspaceId,
                Target = Event,
                Arguments = arguments
            };
    }
}

/// <summary>
/// Encapsulates a user notification.
/// </summary>
public class UserNotification : NotificationBase {
    public string UserId { get; private set; }

    public UserNotification(string userId) {
        UserId = userId;
    }

    protected override SignalRMessage BuildMessage(object[] arguments)
    {
        return new SignalRMessage
            {
                UserId = UserId,
                Target = Event,
                Arguments = arguments
            };
    }
}
