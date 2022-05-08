using User = Zytonomy.Api.Model.User;

namespace Zytonomy.Api.DataAccess.Repositories;

/// <summary>
/// Repository for managing users
/// </summary>
public class UserRepository : CosmosRepositoryBase<User>
{
    /// <summary>
    /// Injection constructor.
    /// </summary>
    /// <param name="cosmos">The instance of the gateway used for connecting to Cosmos</param>
    /// <returns>An instance of the repository.</returns>
    public UserRepository(CosmosGateway cosmos) : base(cosmos)
    {
    }
}