using Flowon.Xrm.Api.AspNetCore;
using Flowon.Xrm.Api.Metadata;

namespace CaseManagementApiHost;

/// <summary>
/// Turns the <c>LogicApi:Version</c> setting into the versioning strategy the host uses, so each
/// environment chooses its version without a code change.
/// </summary>
public static class ApiVersionSetting
{
    /// <summary>
    /// <c>draft</c> serves the draft, <c>latest</c> (or no value) the newest published version,
    /// and <c>major.minor</c>, for example <c>1.0</c>, exactly that version.
    /// </summary>
    public static ApiVersioningStrategy ToStrategy(string? version) => Normalize(version) switch
    {
        "latest" => new LatestVersionApiVersioningStrategy(),
        "draft" => new DraftVersionApiVersioningStrategy(),
        var exact => new ExactVersionApiVersioningStrategy { ApiVersion = Parse(exact) },
    };

    /// <summary>Whether the metadata watcher has anything to pick up for this setting.</summary>
    public static bool IsWatchable(string? version) => Normalize(version) is "latest" or "draft";

    private static string Normalize(string? version) =>
        string.IsNullOrWhiteSpace(version) ? "latest" : version.Trim().ToLowerInvariant();

    private static ApiVersion Parse(string version)
    {
        var parts = version.Split('.');
        if (parts.Length != 2
            || !int.TryParse(parts[0], out var major) || major < 1
            || !int.TryParse(parts[1], out var minor) || minor < 0)
        {
            throw new InvalidOperationException(
                $"LogicApi:Version '{version}' must be 'draft', 'latest' or 'major.minor', for example '1.0'.");
        }

        return new ApiVersion(major, minor);
    }
}
