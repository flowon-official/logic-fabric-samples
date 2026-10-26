using System.Security.Claims;
using Flowon.Xrm;
using Flowon.Xrm.Api;
using Flowon.Xrm.Security;
using EntityReference = Microsoft.Xrm.Sdk.EntityReference;

namespace CaseManagementApiHost;

/// <summary>
/// Tells Logic API who is calling: whether the caller signed in, whether they are a known
/// customer contact, and the <c>contactId</c> user property defined in API Studio under
/// Context &gt; User. The <c>caseSummary</c> query only returns cases whose customer is this
/// contact, so customers see only their own cases.
/// </summary>
public sealed class CustomerUserResolver(ICustomerDirectory customers) : IWebUserResolver
{
    /// <summary>The user property name, exactly as defined in API Studio.</summary>
    public const string ContactIdProperty = "contactId";

    public async Task<IWebUser> ResolveUser(IWebUserType webUserType, ClaimsPrincipal principal)
    {
        // 1. Anonymous caller: no properties.
        if (principal.Identity?.IsAuthenticated != true)
        {
            return webUserType.New(isAuthenticated: false);
        }

        // 2. Signed-in caller: find their contact by the email address in the token.
        var email = GetEmail(principal);
        var contactId = email is null ? null : await customers.FindContactIdAsync(email);

        // 3. Signed in, but not a customer we know: Demand Authenticated User refuses them.
        if (contactId is null)
        {
            return webUserType.New(isAuthenticated: true, isRegistered: false);
        }

        // 4. Known customer: contactId is a Lookup to Contact, so the value is an EntityReference.
        var properties = new Dictionary<string, object>
        {
            [ContactIdProperty] = new EntityReference("contact", contactId.Value),
        };

        return webUserType.New(isAuthenticated: true, isRegistered: true, properties);
    }

    // Entra ID puts the sign-in name in preferred_username; Entra External ID and other
    // providers send an email claim, which ASP.NET Core may map to ClaimTypes.Email.
    private static string? GetEmail(ClaimsPrincipal principal) =>
        new[] { "preferred_username", "email", ClaimTypes.Email, ClaimTypes.Upn }
            .Select(principal.FindFirstValue)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value) && value.Contains('@'));
}
