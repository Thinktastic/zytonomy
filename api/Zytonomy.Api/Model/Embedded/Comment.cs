namespace Zytonomy.Api.Model.Embedded;

/// <summary>
/// A comment is attached to a saved note.  Users may add one or more comments to a note.
/// </summary>
public class Comment
{
    /// <summary>
    /// A reference to the user who created the comment.
    /// </summary>
    public GenericRef Author;

    /// <summary>
    /// The body of the text associated with the user.
    /// </summary>
    public string Body;

    /// <summary>
    /// The ID of the parent entity.  This can be the note or another comment if this is a reply
    /// to a previous comment.
    /// </summary>
    public string ParentId;

    /// <summary>
    /// The UTC date and time that the comment was created.
    /// </summary>
    public string CreatedUtc;
}
