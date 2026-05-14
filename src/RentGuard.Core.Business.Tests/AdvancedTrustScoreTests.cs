using RentGuard.Core.Business.Modules.TrustScore.Domain;
using Xunit;

namespace RentGuard.Core.Business.Tests;

public class AdvancedTrustScoreTests
{
    [Fact]
    public void ColdStart_ShouldReturnNewLevel()
    {
        // 0 meses, 0 score (o score inicial)
        var snapshots = new List<TrustScoreSnapshot>();
        var insight = TrajectoryCalculator.Calculate(snapshots, "tenant-new");

        Assert.Equal(TrustLevel.New, insight.AdjustedTrustLevel);
        Assert.Equal(0, insight.MaturityLevel);
    }

    [Fact]
    public void ExPerfecto_ShouldTriggerVolatilityAndCriticalLevel()
    {
        // Usuario baja de 1000 a 600 en un mes
        var snapshots = new List<TrustScoreSnapshot>
        {
            TrustScoreSnapshot.Create("tenant-1", 1000, DateTime.Now.AddMonths(-1)),
            TrustScoreSnapshot.Create("tenant-1", 600, DateTime.Now)
        };

        var insight = TrajectoryCalculator.Calculate(snapshots, "tenant-1");

        Assert.True(insight.VolatilityFlag);
        Assert.Equal(TrustLevel.Critical, insight.AdjustedTrustLevel);
        Assert.True(insight.TrendVector < 0);
    }

    [Fact]
    public void NuevoPromesa_ShouldHavePositiveTrendAndHighLevel()
    {
        // Usuario sube de 0 a 600 en 6 meses
        var snapshots = new List<TrustScoreSnapshot>();
        for (int i = 0; i <= 6; i++)
        {
            snapshots.Add(TrustScoreSnapshot.Create("tenant-1", i * 100, DateTime.Now.AddMonths(-6 + i)));
        }

        var insight = TrajectoryCalculator.Calculate(snapshots, "tenant-1");

        Assert.False(insight.VolatilityFlag);
        Assert.Equal(TrustLevel.High, insight.AdjustedTrustLevel);
        Assert.True(insight.TrendVector > 0);
    }

    [Fact]
    public void Cooldown_ShouldMaintainVolatilityUntil3MonthsOfPositiveTrend()
    {
        // Caída drástica -> Volatilidad activa
        var snapshots = new List<TrustScoreSnapshot>
        {
            TrustScoreSnapshot.Create("tenant-1", 1000, DateTime.Now.AddMonths(-4)),
            TrustScoreSnapshot.Create("tenant-1", 600, DateTime.Now.AddMonths(-3)), // VolatilityFlag = true
            TrustScoreSnapshot.Create("tenant-1", 610, DateTime.Now.AddMonths(-2)), // Month 1 recovery
            TrustScoreSnapshot.Create("tenant-1", 620, DateTime.Now.AddMonths(-1)), // Month 2 recovery
            TrustScoreSnapshot.Create("tenant-1", 630, DateTime.Now)               // Month 3 recovery
        };

        // En este punto, después de 3 meses de tendencia no negativa, el flag debería desactivarse
        // Pero si solo pasaron 2 meses, debería seguir activo.
        
        var recovery2Months = snapshots.Take(4).ToList();
        var insight2 = TrajectoryCalculator.Calculate(recovery2Months, "tenant-1");
        Assert.True(insight2.VolatilityFlag);

        var recovery3Months = snapshots;
        var insight3 = TrajectoryCalculator.Calculate(recovery3Months, "tenant-1");
        Assert.False(insight3.VolatilityFlag);
    }
}
