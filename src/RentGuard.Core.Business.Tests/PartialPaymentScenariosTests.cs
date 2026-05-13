using RentGuard.Contracts.Modules.TrustScore;
using RentGuard.Core.Business.Modules.TrustScore.Domain;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class PartialPaymentScenariosTests
{
    [Fact]
    public void PartialPayment_ShouldNotChangeScoreValue()
    {
        var score = new TrustScoreEntity("tenant-1", 100);
        
        // Simular evento de pago parcial (Delta = 0)
        score.ApplyDelta(0); 
        
        score.Value.Should().Be(100);
    }

    [Fact]
    public void Sequence_Partial_Then_CompleteOnTime_ShouldIncreaseScore()
    {
        var score = new TrustScoreEntity("tenant-1", 100);
        
        // Paso 1: Inquilino paga el 50%
        score.ApplyDelta(0); // Partial
        
        // Paso 2: Inquilino paga el resto antes de la fecha
        score.ApplyDelta(10); // OnTimePayment
        
        score.Value.Should().Be(110);
    }

    [Fact]
    public void Sequence_Partial_Then_DeadlineExceeded_ShouldDecreaseScore()
    {
        var score = new TrustScoreEntity("tenant-1", 100);
        
        // Paso 1: Inquilino paga el 50%
        score.ApplyDelta(0); // Partial
        
        // Paso 2: Se vence la fecha y no pag el resto
        score.ApplyDelta(-20); // LatePayment (Delay)
        
        score.Value.Should().Be(80);
    }
}
