namespace Zytonomy.Api.DataAccess.Repositories;

/// <summary>
/// Repository for interacting with messages.
/// </summary>
public class MessageRepository : CosmosRepositoryBase<Message>
{
    /// <summary>
    /// Injection constructor.
    /// </summary>
    /// <param name="cosmos">The instance of the gateway used for connecting to Cosmos</param>
    /// <returns>An instance of the repository.</returns>
    public MessageRepository(CosmosGateway cosmos) : base(cosmos)
    {
    }
}
