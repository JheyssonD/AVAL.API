using RentGuard.Core.Business.Modules.TrustScore.Domain;
using Xunit;

namespace RentGuard.Core.Business.Tests;

public class TrajectoryCalculatorTests
{
    [Fact]
    public void ColdStart_ShouldReturnNeutralLevelAt500()
    {
        // 0 months, no snapshots
        var snapshots = new List<TrustScoreSnapshot>();
        var insight = TrajectoryCalculator.Calculate(snapshots, "tenant-new");

        Assert.Equal(TrustLevel.Neutral, insight.AdjustedTrustLevel);
        Assert.Equal(500, insight.CurrentScore);
        Assert.Equal(0, insight.MaturityLevel);
    }

    [Fact]
    public void ExElite_ShouldTriggerAtRiskLevelViaVolatility()
    {
        // User drops from 900 to 500 in one month
        var snapshots = new List<TrustScoreSnapshot>
        {
            TrustScoreSnapshot.Create("tenant-1", 900, DateTime.UtcNow.AddMonths(-1)),
            TrustScoreSnapshot.Create("tenant-1", 500, DateTime.UtcNow)
        };

        var insight = TrajectoryCalculator.Calculate(snapshots, "tenant-1");

        Assert.True(insight.VolatilityFlag);
        Assert.Equal(TrustLevel.AtRisk, insight.AdjustedTrustLevel);
        Assert.True(insight.TrendVector < 0);
    }

    [Fact]
    public void NuevoPromesa_ShouldAscendToElite()
    {
        // User rises from 500 to 900 in 4 months
        var snapshots = new List<TrustScoreSnapshot>();
        for (int i = 0; i <= 4; i++)
        {
            snapshots.Add(TrustScoreSnapshot.Create("tenant-1", 500 + (i * 100), DateTime.UtcNow.AddMonths(-4 + i)));
        }

        var insight = TrajectoryCalculator.Calculate(snapshots, "tenant-1");

        Assert.False(insight.VolatilityFlag);
        Assert.Equal(TrustLevel.Elite, insight.AdjustedTrustLevel);
        Assert.True(insight.TrendVector > 0);
    }

    [Fact]
    public void Cooldown_ShouldMaintainAtRiskUntil3MonthsOfPositiveTrend()
    {
        // Drastic drop -> Volatility active
        var snapshots = new List<TrustScoreSnapshot>
        {
            TrustScoreSnapshot.Create("tenant-1", 1000, DateTime.UtcNow.AddMonths(-4)),
            TrustScoreSnapshot.Create("tenant-1", 500, DateTime.UtcNow.AddMonths(-3)), // VolatilityFlag = true -> AtRisk
            TrustScoreSnapshot.Create("tenant-1", 510, DateTime.UtcNow.AddMonths(-2)), // Month 1 recovery
            TrustScoreSnapshot.Create("tenant-1", 520, DateTime.UtcNow.AddMonths(-1)), // Month 2 recovery
            TrustScoreSnapshot.Create("tenant-1", 530, DateTime.UtcNow)               // Month 3 recovery
        };
        
        var recovery2Months = snapshots.Take(4).ToList();
        var insight2 = TrajectoryCalculator.Calculate(recovery2Months, "tenant-1");
        Assert.True(insight2.VolatilityFlag);
        Assert.Equal(TrustLevel.AtRisk, insight2.AdjustedTrustLevel);

        var recovery3Months = snapshots;
        var insight3 = TrajectoryCalculator.Calculate(recovery3Months, "tenant-1");
        Assert.False(insight3.VolatilityFlag);
        Assert.Equal(TrustLevel.Neutral, insight3.AdjustedTrustLevel);
    }

    [Fact]
    public void Fraud_ShouldReturnBlacklisted()
    {
        var snapshots = new List<TrustScoreSnapshot>
        {
            TrustScoreSnapshot.Create("tenant-1", 500, DateTime.UtcNow.AddMonths(-1)),
            TrustScoreSnapshot.Create("tenant-1", 0, DateTime.UtcNow) // Fraud terminal indicator
        };

        var insight = TrajectoryCalculator.Calculate(snapshots, "tenant-1");

        Assert.Equal(TrustLevel.Blacklisted, insight.AdjustedTrustLevel);
        Assert.Equal(0, insight.CurrentScore);
    }

    [Fact]
    public void PerfectPast_RecentDecline_ShouldHaveNegativeTrend()
    {
        // 6 months of 1000, then 3 months of decline
        var snapshots = new List<TrustScoreSnapshot>();
        for (int i = 0; i < 6; i++) 
            snapshots.Add(TrustScoreSnapshot.Create("user-a", 1000, DateTime.UtcNow.AddMonths(-9 + i)));
        
        snapshots.Add(TrustScoreSnapshot.Create("user-a", 800, DateTime.UtcNow.AddMonths(-2)));
        snapshots.Add(TrustScoreSnapshot.Create("user-a", 600, DateTime.UtcNow.AddMonths(-1)));
        snapshots.Add(TrustScoreSnapshot.Create("user-a", 400, DateTime.UtcNow));

        var insight = TrajectoryCalculator.Calculate(snapshots, "user-a");

        // The slope should be negative despite the long perfect past
        Assert.True(insight.TrendVector < 0, $"Trend should be negative, but was {insight.TrendVector}");
        Assert.True(insight.VolatilityFlag, "Volatility should be triggered by the 1000->800 drop");
        Assert.Equal(TrustLevel.AtRisk, insight.AdjustedTrustLevel);
    }

    [Fact]
    public void PoorPast_RecentRecovery_ShouldHavePositiveTrend()
    {
        // 6 months of 200, then 3 months of recovery
        var snapshots = new List<TrustScoreSnapshot>();
        for (int i = 0; i < 6; i++) 
            snapshots.Add(TrustScoreSnapshot.Create("user-b", 200, DateTime.UtcNow.AddMonths(-9 + i)));
        
        snapshots.Add(TrustScoreSnapshot.Create("user-b", 400, DateTime.UtcNow.AddMonths(-2)));
        snapshots.Add(TrustScoreSnapshot.Create("user-b", 600, DateTime.UtcNow.AddMonths(-1)));
        snapshots.Add(TrustScoreSnapshot.Create("user-b", 800, DateTime.UtcNow));

        var insight = TrajectoryCalculator.Calculate(snapshots, "user-b");

        // The slope should be positive
        Assert.True(insight.TrendVector > 0, $"Trend should be positive, but was {insight.TrendVector}");
        Assert.Equal(TrustLevel.Neutral, insight.AdjustedTrustLevel); // Not yet Elite, but recovering
    }
}
