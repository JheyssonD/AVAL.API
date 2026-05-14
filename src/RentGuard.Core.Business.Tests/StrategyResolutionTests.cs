using Moq;
using RentGuard.Core.Business.Modules.TrustScore.Domain;
using RentGuard.Core.Business.Modules.TrustScore.Domain.Repositories;
using RentGuard.Core.Business.Modules.TrustScore.Domain.Strategies;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class StrategyResolutionTests
{
    private readonly Mock<ITenantSettingsRepository> _settingsRepoMock;
    private readonly List<ITrustScoreCalculationStrategy> _strategies;
    private readonly TrustScoreEngineResolver _resolver;

    public StrategyResolutionTests()
    {
        _settingsRepoMock = new Mock<ITenantSettingsRepository>();
        
        var legacyMock = new Mock<ITrustScoreCalculationStrategy>();
        legacyMock.Setup(s => s.StrategyName).Returns("Legacy");
        
        var advancedMock = new Mock<ITrustScoreCalculationStrategy>();
        advancedMock.Setup(s => s.StrategyName).Returns("AdvancedTrajectory");

        _strategies = new List<ITrustScoreCalculationStrategy> { legacyMock.Object, advancedMock.Object };
        _resolver = new TrustScoreEngineResolver(_strategies, _settingsRepoMock.Object);
    }

    [Fact]
    public async Task GetStrategy_ShouldReturnLegacy_WhenConfiguredAsLegacy()
    {
        var tenantId = Guid.NewGuid();
        _settingsRepoMock.Setup(r => r.GetTrustEngineTypeAsync(tenantId)).ReturnsAsync("Legacy");

        var strategy = await _resolver.GetStrategyForTenantAsync(tenantId);

        strategy.StrategyName.Should().Be("Legacy");
    }

    [Fact]
    public async Task GetStrategy_ShouldReturnAdvanced_WhenConfiguredAsAdvanced()
    {
        var tenantId = Guid.NewGuid();
        _settingsRepoMock.Setup(r => r.GetTrustEngineTypeAsync(tenantId)).ReturnsAsync("AdvancedTrajectory");

        var strategy = await _resolver.GetStrategyForTenantAsync(tenantId);

        strategy.StrategyName.Should().Be("AdvancedTrajectory");
    }

    [Fact]
    public async Task GetStrategy_ShouldFallbackToLegacy_WhenConfigurationIsMissing()
    {
        var tenantId = Guid.NewGuid();
        _settingsRepoMock.Setup(r => r.GetTrustEngineTypeAsync(tenantId)).ReturnsAsync((string)null!);

        var strategy = await _resolver.GetStrategyForTenantAsync(tenantId);

        strategy.StrategyName.Should().Be("Legacy");
    }
}
