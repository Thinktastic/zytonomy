using Member = Zytonomy.Api.Model.Embedded.Member;
using User = Zytonomy.Api.Model.User;

namespace Zytonomy.Api.DataAccess.Mutators;

/// <summary>
/// Mutator for user and workspace entities.
/// </summary>
public class UserWorkspaceMutator
{
    private WorkspaceRepository _workspaces;
    private UserRepository _users;

    /// <summary>
    /// Injection constructor.
    /// </summary>
    public UserWorkspaceMutator(
        WorkspaceRepository workspaces,
        UserRepository users)
    {
        _workspaces = workspaces;
        _users = users;
    }

    /// <summary>
    /// Adds the user to the workspace and vice versa.
    /// </summary>
    public async void ConnectUserAndWorkspace(EntityRelation relation, Action<User> clientSignal)
    {
        User user = await _users.GetByIdAsync(relation.ParentEntityRef.Id);

        if(user.Workspaces == null)
        {
            user.Workspaces = new List<GenericRef>();
        }

        user.Workspaces.Add(relation.EmbeddedEntityRef);

        await _users.UpsertAsync(user);

        // Now add the user to the workspace.
        // TODO: Consider making this two discrete events.
        Workspace workspace = await _workspaces.GetByIdAsync(relation.EmbeddedEntityRef.Id);

        if(workspace.Members == null)
        {
            workspace.Members = new List<Member>();
        }

        workspace.Members.Add(new Member {
            User = new GenericRef(user.Id, $"{user.FirstName} {user.LastName}"),
            AddedUtc = DateTime.UtcNow.ToString("u")
        });

        await _workspaces.UpsertAsync(workspace);

        clientSignal(user);
    }
}
