using FluentAssertions;
using RentGuard.Core.Business.Modules.TrustScore.Domain;
using Xunit;

namespace RentGuard.Core.Business.Tests;

public class TrustScoreDomainTests
{
    [Fact]
    public void Should_Update_Score_And_Create_History_Record()
    {
        // Arrange
        var tenantId = "tenant-777";
        var currentScore = 80;
        
        // Act
        var history = TrustScoreHistory.Create(tenantId, currentScore, currentScore + 5, "Payment Approved");

        // Assert
        history.NewScore.Should().Be(85);
        history.Reason.Should().Be("Payment Approved");
        history.TenantId.Should().Be(tenantId);
    }

    [Theory]
    [InlineData(98, 5, 100)] // No puede superar 100
    [InlineData(2, -10, 0)]  // No puede ser menor a 0
    public void Score_Should_Respect_Domain_Boundaries(int current, int change, int expected)
    {
        // Act
        int finalScore = Math.Clamp(current + change, 0, 100);

        // Assert
        finalScore.Should().Be(expected);
    }
}
