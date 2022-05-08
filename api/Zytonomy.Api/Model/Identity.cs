namespace Zytonomy.Api.Model;

/// <summary>
/// Reprents the identity of the currently logged in user.
/// </summary>
public class Identity
{
    private const string _oidClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    private const string _emailClaim = "emails";
    private const string _firstNameClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
    private const string _lastNameClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
    private const string _idpClaim = "http://schemas.microsoft.com/identity/claims/identityprovider";

    /// <summary>
    /// Gets a value indicating whether the identity is valid.
    /// </summary>
    /// <value>True if the identity was able to be parsed from the claims principal.</value>
    public bool IsValid { get; private set; }

    /// <summary>
    /// A GUID in string format that represents the ID of the user.
    /// </summary>
    public readonly string Id;

    /// <summary>
    /// The email address associated with the user.
    /// </summary>
    public readonly string Email;

    /// <summary>
    /// The first name of the user.
    /// </summary>
    public readonly string FirstName;

    /// <summary>
    /// The last name of the user.
    /// </summary>
    public readonly string LastName;

    /// <summary>
    /// The identity provider which authenticated the user (e.g. "google.com")
    /// </summary>
    public readonly string IdP;

    /// <summary>
    /// Instantiates a new instance of identity using the input claims.
    /// </summary>
    /// <param name="claimsPrincipal">The input claims associated with the context user.</param>
    public Identity(ClaimsPrincipal claimsPrincipal) {
        Id = claimsPrincipal.FindFirst(c => c.Type == _oidClaim).Value;
        Email = claimsPrincipal.FindFirst(c => c.Type == _emailClaim).Value;
        FirstName = claimsPrincipal.FindFirst(c => c.Type == _firstNameClaim).Value;
        LastName = claimsPrincipal.FindFirst(c => c.Type == _lastNameClaim).Value;
        IsValid = true;
    }

    /// <summary>
    /// Gets the identity as a generic reference for use in embedding into other documents.
    /// </summary>
    public GenericRef GenericRef {
        get {
            return new GenericRef {
                Id = Id,
                Name = $"{FirstName} {LastName}"
            };
        }
    }
}