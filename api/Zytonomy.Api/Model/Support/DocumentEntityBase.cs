namespace Zytonomy.Api.Model;

/// <summary>
/// Abstract base class for domain entities which are persisted to the database.
/// </summary>
public abstract class DocumentEntityBase : IEntity
{
    /// <summary>
    /// A system assigned ID of the entity.
    /// Need to add the NewtonsoftJson attribute for queries to work; not sure if the below will fix it
    /// Possibly addressed in future release so keep JsonPropertyName for now.
    /// See: https://github.com/Azure/azure-cosmos-dotnet-v3/issues/484#issuecomment-715373957
    /// </summary>
    [JsonPropertyName("id")]
    [Newtonsoft.Json.JsonProperty("id")]
    public virtual string Id { get; set; }

    /// <summary>
    /// A name associated with the entity.
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// The partition key for this type.
    /// </summary>
    public abstract string PartitionKey { get; }

    /// <summary>
    /// Gets the container name based on either the type name or an explicit container name using the Container attribute.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public virtual string ContainerName
    {
        get
        {
            ContainerAttribute containerAttribute = GetType().GetCustomAttribute<ContainerAttribute>();

            return string.IsNullOrEmpty(containerAttribute?.Name) ? GetType().Name : containerAttribute.Name;
        }
    }

    /// <summary>
    /// Returns the type name.
    /// </summary>
    public virtual string TypeName
    {
        get => GetType().Name;
        set { /* Needed for serialization */ }
    }

    /// <summary>
    /// Gets the entity as a generic reference for use in embedding into other documents.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public virtual GenericRef GenericRef {
        get {
            return new GenericRef (
                Id,
                Name,
                ContainerName,
                PartitionKey
            );
        }
    }
}
