namespace Zytonomy.Api.Model.Embedded;

/// <summary>
/// Represents a generic reference to another entity that is embedded into a document.
/// </summary>
public class GenericRef : IEntity
{
    public GenericRef() {

    }

    public GenericRef(string id, string name)
    {
        this.Id = id;
        this.Name = name;
    }

    public GenericRef(string id, string name, string containerName, string partitionKey)
    {
        this.Id = id;
        this.Name = name;
        this.ContainerName = containerName;
        this.PartitionKey = partitionKey;
    }

    public string Id { get; set; }

    public string Name { get; set; }

    public string ContainerName { get; set; }

    public string PartitionKey { get; set; }

    /// <summary>
    /// Convenience method for creating an entity relation from this reference to another reference.
    /// </summary>
    /// <param name="embed">The entity to embed.</param>
    /// <returns>An instance of entity relation with the root entity as the parent.</returns>
    public virtual EntityRelation Embed(GenericRef embed)
    {
        return new EntityRelation(this, embed);
    }

    /// <summary>
    /// Convenience method for creating an entity relation from this reference to another reference.
    /// </summary>
    /// <param name="embed">The entity to embed.</param>
    /// <returns>An instance of entity relation with the root entity as the parent.</returns>
    public virtual EntityRelation Embed<T>(T embed) where T : DocumentEntityBase
    {
        return new EntityRelation(this, embed.GenericRef);
    }
}
