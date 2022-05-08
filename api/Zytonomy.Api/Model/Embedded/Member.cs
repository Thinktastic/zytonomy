namespace Zytonomy.Api.Model.Embedded;

/// <summary>
/// Embedded object in a workspace where a user is a member.
/// </summary>
public class Member
{
    /// <summary>
    /// The roles associated with the user in the workspace.
    /// </summary>
    private List<string> Roles;

    /// <summary>
    /// The user reference.
    /// </summary>
    public GenericRef User;

    /// <summary>
    /// The UTC date and time string that the user was added to the workspace.
    /// </summary>
    public string AddedUtc;
}
