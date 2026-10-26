namespace CaseManagementApiHost;

/// <summary>
/// Finds the Dataverse contact that a signed-in customer represents.
/// </summary>
public interface ICustomerDirectory
{
    /// <summary>
    /// Returns the ID of the active contact with this email address, or <c>null</c> when there
    /// is none.
    /// </summary>
    Task<Guid?> FindContactIdAsync(string email, CancellationToken cancellationToken = default);
}
