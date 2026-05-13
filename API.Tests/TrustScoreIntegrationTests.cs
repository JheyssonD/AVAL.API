using Moq;
using RentGuard.Core.Business.Modules.TrustScore.GetTrustScore;
using RentGuard.Core.Business.Modules.TrustScore.Domain.Repositories;
using FluentAssertions;
using Xunit;

namespace RentGuard.API.Tests;

public class TrustScoreIntegrationTests
{
    private readonly Mock<ITrustScoreRepository> _repoMock = new();
    private readonly GetTrustScoreHandler _handler;

    public TrustScoreIntegrationTests()
    {
        _handler = new GetTrustScoreHandler(_repoMock.Object);
    }

    [Fact]
    public async Task GetTrustScore_Should_Return_Score_From_Repository()
    {
        var tenantId = "tenant-1";
        _repoMock.Setup(x => x.GetCurrentScoreAsync(tenantId)).ReturnsAsync(95);

        var response = await _handler.Handle(tenantId, CancellationToken.None);

        response.Should().Be(95);
    }
}
