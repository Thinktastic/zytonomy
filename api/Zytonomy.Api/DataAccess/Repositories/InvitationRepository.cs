namespace Zytonomy.Api.DataAccess.Repositories;

/// <summary>
/// Repository for managing invitations.
/// </summary>
public class InvitationRepository : CosmosRepositoryBase<Invitation>
{
    /// <summary>
    /// Injection constructor.
    /// </summary>
    /// <param name="cosmos">The instance of the gateway used for connecting to Cosmos</param>
    /// <returns>An instance of the repository.</returns>
    public InvitationRepository(CosmosGateway cosmos) : base(cosmos)
    {
    }
}
