using Flowon.Xrm.Api.AspNetCore;

namespace CaseManagementApiHost.Tests;

public class ApiVersionSettingTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("latest")]
    [InlineData(" Latest ")]
    public void Latest_or_no_value_serves_the_newest_published_version(string? setting)
    {
        Assert.IsType<LatestVersionApiVersioningStrategy>(ApiVersionSetting.ToStrategy(setting));
        Assert.True(ApiVersionSetting.IsWatchable(setting));
    }

    [Theory]
    [InlineData("draft")]
    [InlineData("DRAFT")]
    public void Draft_serves_the_draft(string setting)
    {
        Assert.IsType<DraftVersionApiVersioningStrategy>(ApiVersionSetting.ToStrategy(setting));
        Assert.True(ApiVersionSetting.IsWatchable(setting));
    }

    [Fact]
    public void A_version_number_serves_exactly_that_version()
    {
        var strategy = Assert.IsType<ExactVersionApiVersioningStrategy>(ApiVersionSetting.ToStrategy("2.1"));

        Assert.Equal(2, strategy.ApiVersion.Major);
        Assert.Equal(1, strategy.ApiVersion.Minor);
        Assert.False(ApiVersionSetting.IsWatchable("2.1"));
    }

    [Theory]
    [InlineData("2")]
    [InlineData("2.1.0")]
    [InlineData("v2.1")]
    [InlineData("0.1")]
    [InlineData("newest")]
    public void Anything_else_is_refused_with_a_clear_message(string setting)
    {
        var error = Assert.Throws<InvalidOperationException>(() => ApiVersionSetting.ToStrategy(setting));

        Assert.Contains("LogicApi:Version", error.Message);
    }
}
