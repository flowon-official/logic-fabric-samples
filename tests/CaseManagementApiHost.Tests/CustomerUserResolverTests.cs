using System.Security.Claims;
using Flowon.Xrm;
using Flowon.Xrm.Security;
using NSubstitute;

namespace CaseManagementApiHost.Tests;

public class CustomerUserResolverTests
{
    private static readonly Guid ContactId = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    private readonly IWebUserType userType = Substitute.For<IWebUserType>();
    private readonly ICustomerDirectory customers = Substitute.For<ICustomerDirectory>();

    public CustomerUserResolverTests()
    {
        customers.FindContactIdAsync("jane@contoso.com", Arg.Any<CancellationToken>()).Returns(ContactId);
    }

    [Fact]
    public async Task A_caller_without_a_token_is_anonymous()
    {
        await Resolve(new ClaimsPrincipal(new ClaimsIdentity()));

        userType.Received(1).New(false);
        await customers.DidNotReceiveWithAnyArgs().FindContactIdAsync(default!, default);
    }

    [Fact]
    public async Task A_known_customer_is_registered_with_their_contact_id()
    {
        await Resolve(SignedIn(("preferred_username", "jane@contoso.com")));

        userType.Received(1).New(true, true, Arg.Is<IReadOnlyDictionary<string, object>>(p =>
            p.Count == 1 && ContactId.Equals(p[CustomerUserResolver.ContactIdProperty])));
    }

    [Fact]
    public async Task The_email_claim_is_used_when_there_is_no_sign_in_name()
    {
        await Resolve(SignedIn((ClaimTypes.Email, "jane@contoso.com")));

        userType.Received(1).New(true, true, Arg.Any<IReadOnlyDictionary<string, object>>());
    }

    [Fact]
    public async Task A_signed_in_caller_who_is_not_a_customer_is_not_registered()
    {
        await Resolve(SignedIn(("preferred_username", "someone@fabrikam.com")));

        userType.Received(1).New(true, false);
    }

    [Fact]
    public async Task A_token_without_an_email_address_is_not_registered()
    {
        await Resolve(SignedIn(("preferred_username", "service-principal")));

        userType.Received(1).New(true, false);
        await customers.DidNotReceiveWithAnyArgs().FindContactIdAsync(default!, default);
    }

    private Task<IWebUser> Resolve(ClaimsPrincipal principal) =>
        new CustomerUserResolver(customers).ResolveUser(userType, principal);

    private static ClaimsPrincipal SignedIn(params (string Type, string Value)[] claims) =>
        new(new ClaimsIdentity(claims.Select(c => new Claim(c.Type, c.Value)), authenticationType: "Bearer"));
}
