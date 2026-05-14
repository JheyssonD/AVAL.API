using RentGuard.Core.Business.Modules.Payments.Domain;
using Xunit;

namespace RentGuard.Core.Business.Tests;

public class PaymentFsmTests
{
    [Fact]
    public void NewPayment_ShouldStartInReceivedState()
    {
        var payment = Payment.Create(Guid.NewGuid(), 100, DateTime.UtcNow, "REF");
        Assert.Equal(PaymentStatus.Received, payment.Status);
    }

    [Fact]
    public void LegalTransition_ShouldSucceed()
    {
        var payment = Payment.Create(Guid.NewGuid(), 100, DateTime.UtcNow, "REF");
        
        payment.Process();
        Assert.Equal(PaymentStatus.Processing, payment.Status);
        
        payment.Approve();
        Assert.Equal(PaymentStatus.Approved, payment.Status);
    }

    [Fact]
    public void IllegalTransition_ShouldThrowDomainException()
    {
        var payment = Payment.Create(Guid.NewGuid(), 100, DateTime.UtcNow, "REF");
        payment.Process();
        payment.Approve();

        // Approved is terminal. Cannot reject.
        var ex = Assert.Throws<DomainException>(() => payment.Reject());
        Assert.Equal(DomainErrors.Payments.InvalidTransition.Code, ex.Error.Code);
    }

    [Fact]
    public void DuplicateDetection_ShouldTransitionToDuplicate()
    {
        var payment = Payment.Create(Guid.NewGuid(), 100, DateTime.UtcNow, "REF");
        payment.Process();
        
        payment.MarkAsDuplicate("HASH-123");
        
        Assert.Equal(PaymentStatus.Duplicate, payment.Status);
        Assert.Equal("HASH-123", payment.ImageHash);
    }
}
