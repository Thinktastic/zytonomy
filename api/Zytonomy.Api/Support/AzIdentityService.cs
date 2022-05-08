namespace Zytonomy.Api.Support;

/// <summary>
/// The identity service caches the information for processing the idToken from the client
/// which contains the claims about the user.  This class acts to cache the key information
/// from the Azure AD B2C endpoint required to decrypt the token.
/// </summary>
public class AzIdentityService
{
    private List<SecurityKey> _keys;

    /// <summary>
    /// Creates an instance of the identity service using the keys from the identity authority endpoint.
    /// </summary>
    /// <param name="keys">The security keys extracted from the authority metadata.</param>
    private AzIdentityService(List<SecurityKey> keys) {
        _keys = keys;
    }

    /// <summary>
    /// Retrieves the claims principal based on an input base64 idToken string.  This string is provided
    /// by Azure AD B2C and contains the set of claims about the user.  This method will decrypt the string
    /// using the cached key information obtained from Azure AD B2C.
    /// </summary>
    /// <param name="base64IdToken">The base64 encoded idToken value returned from AzureAD B2C during authentication.</param>
    /// <returns>A ClaimsPrincipal which encapsulates the claims encoded in the string.</returns>
    public ClaimsPrincipal GetClaimsPrincipal(string base64IdToken) {
        SecurityToken validatedToken = null;

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler(); // <-- System.IdentityModel.Tokens.Jwt

        ClaimsPrincipal claimsPrincipal = handler.ValidateToken(base64IdToken,
            new TokenValidationParameters // <-- Microsoft.IdentityModel.Tokens
            {
                // TODO: Validate these in a future stage; enough to decode the claims for now.
                ValidateIssuer = false,
                ValidateLifetime = false,
                ValidateAudience = false,
                IssuerSigningKeys = _keys
            }, out validatedToken);

        return claimsPrincipal;
    }

    /// <summary>
    /// Initializes an instance of the identity service using the key configuration for the identity
    /// federation gateway (Azure AD B2C).  This requires an HTTP request to retrieve the metadata JSON
    /// for the keys so we want to cache this if possible.
    /// </summary>
    /// <returns>An instance of the identity service.</returns>
    public static AzIdentityService Initialize() {
        // Get the OIDC configuration from Azure AD B2C
        // TODO: Move this to configuration.
        string authority = "https://zytonomy.b2clogin.com/zytonomy.onmicrosoft.com/b2c_1_registration/discovery/keys";

        HttpClient client = new HttpClient();

        string metadataJson = client.GetStringAsync(authority).Result;

        List<SecurityKey> keys = GetSecurityKeys(new JsonWebKeySet(metadataJson));

        return new AzIdentityService(keys);
    }

    #region Helper methods

    private static List<SecurityKey> GetSecurityKeys(JsonWebKeySet jsonWebKeySet)
    {
        var keys = new List<SecurityKey>();

        foreach (var key in jsonWebKeySet.Keys)
        {
            if (key.Kty == "RSA")
            {
                if (key.X5c != null && key.X5c.Count > 0)
                {
                    string certificateString = key.X5c[0];
                    var certificate = new X509Certificate2(Convert.FromBase64String(certificateString));

                    var x509SecurityKey = new X509SecurityKey(certificate)
                    {
                        KeyId = key.Kid
                    };

                    keys.Add(x509SecurityKey);
                }
                else if (!string.IsNullOrWhiteSpace(key.E) && !string.IsNullOrWhiteSpace(key.N))
                {
                    byte[] exponent = Base64UrlDecode(key.E);
                    byte[] modulus = Base64UrlDecode(key.N);

                    var rsaParameters = new RSAParameters
                    {
                        Exponent = exponent,
                        Modulus = modulus
                    };

                    var rsaSecurityKey = new RsaSecurityKey(rsaParameters)
                    {
                        KeyId = key.Kid
                    };

                    keys.Add(rsaSecurityKey);
                }
                else
                {
                    throw new Exception("JWK data is missing in token validation");
                }
            }
            else
            {
                throw new NotImplementedException("Only RSA key type is implemented for token validation");
            }
        }

        return keys;
    }

    private static byte[] Base64UrlDecode(string arg)
    {
        string s = arg;

        s = s.Replace('-', '+'); // 62nd char of encoding
        s = s.Replace('_', '/'); // 63rd char of encoding

        switch (s.Length % 4) // Pad with trailing '='s
        {
            case 0: break; // No pad chars in this case
            case 2: s += "=="; break; // Two pad chars
            case 3: s += "="; break; // One pad char
            default:
                throw new System.Exception(
            "Illegal base64url string!");
        }

        return Convert.FromBase64String(s); // Standard base64 decoder
    }

    #endregion
}
