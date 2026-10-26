namespace CaseManagementApiHost;

/// <summary>
/// The <c>LogicApi</c> section of the configuration: which API this host serves.
/// </summary>
public sealed class LogicApiOptions
{
    public const string SectionName = "LogicApi";

    /// <summary>The Logic Fabric project, exactly as named in the Logic Fabric API Builder app.</summary>
    public string Project { get; set; } = "Case Management";

    /// <summary>The API, exactly as named in the Logic Fabric API Builder app.</summary>
    public string Api { get; set; } = "Customer Portal API";

    /// <summary><c>draft</c>, <c>latest</c> or a published version such as <c>1.0</c>.</summary>
    public string Version { get; set; } = "latest";

    /// <summary>
    /// How often, in minutes, to check for a changed draft or a newer version. <c>0</c> turns
    /// the check off, so the host keeps what it loaded at startup.
    /// </summary>
    public int MetadataWatchIntervalMinutes { get; set; } = 5;
}
