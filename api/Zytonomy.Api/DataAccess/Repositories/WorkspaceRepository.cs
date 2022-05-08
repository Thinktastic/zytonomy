namespace Zytonomy.Api.DataAccess.Repositories;

/// <summary>
/// Repository for managing content sets.
/// </summary>
public class WorkspaceRepository : CosmosRepositoryBase<Workspace>
{
    /// <summary>
    /// Injection constructor.
    /// </summary>
    /// <param name="cosmos">The instance of the gateway used for connecting to Cosmos</param>
    /// <returns>An instance of the repository.</returns>
    public WorkspaceRepository(CosmosGateway cosmos) : base(cosmos)
    {
    }
}
