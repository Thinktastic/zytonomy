namespace Zytonomy.Api.Model.Support;

/// <summary>
/// Used for modelling an entity relation whereby an entity reference is to be embedded
/// into another parent entity.  This is used when passing mutations via Queues.
/// </summary>
public class EntityRelation
{
    /// <summary>
    /// The reference to the parent entity.
    /// </summary>
    public GenericRef ParentEntityRef;

    /// <summary>
    /// The reference to the embedded entity.
    /// </summary>
    public GenericRef EmbeddedEntityRef;

    /// <summary>
    /// Default constructor for serialization.
    /// </summary>
    public EntityRelation() {

    }

    /// <summary>
    /// Creates an instance linking a parent to an embedded reference.
    /// </summary>
    /// <param name="parent">The reference to the parent entity.</param>
    /// <param name="embed">The reference to the embedded entity.</param>
    public EntityRelation(GenericRef parent, GenericRef embed) {
        ParentEntityRef = parent;
        EmbeddedEntityRef = embed;
    }
}
