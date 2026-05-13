using RentGuard.Contracts.Modules.TrustScore;
using RentGuard.Core.Business.Modules.TrustScore.Domain;
using Xunit;

namespace RentGuard.Core.Business.Tests;

public class TrustScoreTests
{
    [Fact]
    public void InitialScore_ShouldBe100()
    {
        var score = new TrustScoreEntity("tenant-1");
        Assert.Equal(100, score.Value);
    }

    [Fact]
    public void Score_ShouldNotExceed1000()
    {
        var score = new TrustScoreEntity("tenant-1", 995);
        score.ApplyDelta(15); // Pago anticipado
        Assert.Equal(1000, score.Value);
    }

    [Fact]
    public void Score_ShouldNotBeNegative()
    {
        var score = new TrustScoreEntity("tenant-1", 10);
        score.ApplyDelta(-20); // Pago tardo
        Assert.Equal(0, score.Value);
    }

    [Theory]
    [InlineData(TrustScoreEvent.EarlyPayment, 15)]
    [InlineData(TrustScoreEvent.OnTimePayment, 10)]
    [InlineData(TrustScoreEvent.LatePayment, -20)]
    [InlineData(TrustScoreEvent.RejectedPayment, -5)]
    public void Deltas_ShouldBeCorrectlyApplied(TrustScoreEvent ev, int expectedDelta)
    {
        var score = new TrustScoreEntity("tenant-1", 100);
        int initialValue = score.Value;
        
        // Simular lo que hace el handler
        int delta = ev switch
        {
            TrustScoreEvent.EarlyPayment => 15,
            TrustScoreEvent.OnTimePayment => 10,
            TrustScoreEvent.LatePayment => -20,
            TrustScoreEvent.RejectedPayment => -5,
            _ => 0
        };
        
        score.ApplyDelta(delta);
        Assert.Equal(initialValue + expectedDelta, score.Value);
    }
}
