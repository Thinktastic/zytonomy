namespace Zytonomy.Api.DataAccess.Repositories;

/// <summary>
/// Repository class to manage interacting with notes in Cosmos.
/// </summary>
public class NoteRepository : CosmosRepositoryBase<Note>
{
    /// <summary>
    /// Injection constructor.
    /// </summary>
    /// <param name="cosmos">The instance of the gateway used for connecting to Cosmos</param>
    /// <returns>An instance of the repository.</returns>
    public NoteRepository(CosmosGateway cosmos) : base(cosmos)
    {
    }
}
