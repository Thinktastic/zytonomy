namespace Zytonomy.Api.Model;

/// <summary>
///     Interface for an entity.
/// </summary>
public interface IEntity
{
    string Id { get; }

    string Name { get; }

    string PartitionKey { get; }

    string ContainerName { get; }
}
