using FluentAssertions;
using RentGuard.Core.Business.Modules.Payments.Domain;
using Xunit;

namespace RentGuard.Core.Business.Tests;

public class PaymentDomainTests
{
    [Fact]
    public void Should_Create_Pending_Payment()
    {
        // Arrange
        var leaseId = Guid.NewGuid();
        var amount = 1000m;
        var date = DateTime.UtcNow;

        // Act
        var payment = Payment.Create(leaseId, amount, date, "REF-123");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Amount.Should().Be(1000m);
    }

    [Fact]
    public void Should_Throw_Exception_When_Amount_Is_Zero_Or_Negative()
    {
        // Act
        Action act = () => Payment.Create(Guid.NewGuid(), 0, DateTime.UtcNow, "REF");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*greater than zero*");
    }

    [Fact]
    public void Should_Transition_To_Approved_Successfully()
    {
        // Arrange
        var payment = Payment.Create(Guid.NewGuid(), 1000, DateTime.UtcNow, "REF");

        // Act
        payment.Approve();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Approved);
    }
}
