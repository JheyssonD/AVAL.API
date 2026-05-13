using RentGuard.Contracts.Modules.TrustScore;
using RentGuard.Core.Business.Modules.TrustScore.GetTrustScore;
using FluentAssertions;
using Xunit;

namespace RentGuard.API.Tests;

public class TrustScoreIntegrationTests
{
    private readonly GetTrustScoreHandler _handler = new();

    [Fact]
    public async Task GetTrustScore_ShouldReturnInitialScore()
    {
        var req = new GetTrustScoreRequest("tenant-1");
        var response = await _handler.Handle(req);

        response.Should().NotBeNull();
        response.CurrentScore.Should().Be(100);
        response.Tier.Should().Be("AtRisk");
    }
}
