namespace Zytonomy.Api.Model;

/// <summary>
/// Models an invitation that is requested via the API to add a user to the workspace.
/// </summary>
[Container("Core")]
public class Invitation : DocumentEntityBase
{
    /// <summary>
    /// The email address of the user invited.
    /// </summary>
    public string Email;

    /// <summary>
    /// The first name of the user invited.
    /// </summary>
    public string FirstName;

    /// <summary>
    /// The last name of the user invited.
    /// </summary>
    public string LastName;

    /// <summary>
    /// A 600 character message to be sent to the user.
    /// </summary>
    public string Message;

    /// <summary>
    /// The user that generated the invitation.
    /// </summary>
    public GenericRef InvitedBy;

    /// <summary>
    /// The date and time when the invitation was created.
    /// </summary>
    public string CreatedUtc;

    /// <summary>
    /// The status of the invitation.
    /// </summary>
    public string Status;

    /// <summary>
    /// The ID of the workspace that the user is being inited to.
    /// </summary>
    public string WorkspaceId;

    public override string PartitionKey => WorkspaceId;
}
