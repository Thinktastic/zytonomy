namespace Zytonomy.Api.Model;

/// <summary>
/// Represents a user entity.
/// </summary>
[Container("Core")]
public class User : DocumentEntityBase
{
    /// <summary>
    /// The email address associated with the user.
    /// </summary>
    public string Email;

    /// <summary>
    /// The first name associated with the user.
    /// </summary>
    public string FirstName;

    /// <summary>
    /// The last name associated with the user.
    /// </summary>
    public string LastName;

    /// <summary>
    /// Creates an instance of a user.
    /// </summary>
    public User() {

    }

    /// <summary>
    /// Reuse the ID as the partition key.
    /// </summary>
    public override string PartitionKey => Id;

    /// <summary>
    /// Overrides the Name and sets it to the Email since we don't need this property.
    /// </summary>
    /// <value>The unique name for this entity (Name)</value>
    public override string Name {
        get => Email;
        set => base.Name = value;
    }

    /// <summary>
    /// The list of workspaces that the user is associated with.
    /// </summary>
    public List<GenericRef> Workspaces { get; set; }
}