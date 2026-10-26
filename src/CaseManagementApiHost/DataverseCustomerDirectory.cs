using Microsoft.Extensions.Caching.Memory;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace CaseManagementApiHost;

/// <summary>
/// Looks customers up in the Contact table, using the host's own Dataverse connection (the one
/// created in <c>AddFlowonApi</c>). Results are cached per email address, so a busy customer
/// costs one Dataverse query, not one per request.
/// </summary>
public sealed class DataverseCustomerDirectory(
    IOrganizationServiceAsync2 dataverse,
    IMemoryCache cache,
    IConfiguration configuration) : ICustomerDirectory
{
    // A contact who is deactivated keeps access until their cached entry expires.
    private readonly TimeSpan cacheDuration =
        TimeSpan.FromMinutes(configuration.GetValue("Customers:CacheMinutes", 10));

    public async Task<Guid?> FindContactIdAsync(string email, CancellationToken cancellationToken = default)
    {
        var key = $"contact:{email.Trim().ToLowerInvariant()}";

        return await cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheDuration;
            return await QueryContactIdAsync(email.Trim(), cancellationToken);
        });
    }

    private async Task<Guid?> QueryContactIdAsync(string email, CancellationToken cancellationToken)
    {
        var query = new QueryExpression("contact")
        {
            ColumnSet = new ColumnSet("contactid"),
            TopCount = 1,
            Criteria =
            {
                Conditions =
                {
                    new ConditionExpression("emailaddress1", ConditionOperator.Equal, email),
                    new ConditionExpression("statecode", ConditionOperator.Equal, 0),
                },
            },
        };

        var result = await dataverse.RetrieveMultipleAsync(query, cancellationToken);
        return result.Entities.FirstOrDefault()?.Id;
    }
}
