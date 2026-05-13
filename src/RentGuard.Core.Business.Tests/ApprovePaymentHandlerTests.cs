using Moq;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using RentGuard.Core.Business.Modules.TrustScore.Domain;
using RentGuard.Core.Business.Modules.TrustScore.Domain.Repositories;
using RentGuard.Core.Business.Modules.Payments.ApprovePayment;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class ApprovePaymentHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<ILeaseRepository> _leaseRepoMock;
    private readonly Mock<ITrustScoreRepository> _trustScoreRepoMock;
    private readonly ApprovePaymentHandler _handler;

    public ApprovePaymentHandlerTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _leaseRepoMock = new Mock<ILeaseRepository>();
        _trustScoreRepoMock = new Mock<ITrustScoreRepository>();
        _handler = new ApprovePaymentHandler(_paymentRepoMock.Object, _leaseRepoMock.Object, _trustScoreRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Approve_Payment_Update_Score_And_Add_History()
    {
        var leaseId = Guid.NewGuid();
        var tenantId = "tenant-123";
        var payment = Payment.Create(leaseId, 1000, DateTime.UtcNow, "REF");
        var lease = Lease.Create(leaseId, tenantId, DateTime.UtcNow, 5);

        _paymentRepoMock.Setup(x => x.GetByIdAsync(payment.Id)).ReturnsAsync(payment);
        _leaseRepoMock.Setup(x => x.GetByIdAsync(leaseId)).ReturnsAsync(lease);
        _trustScoreRepoMock.Setup(x => x.GetCurrentScoreAsync(tenantId)).ReturnsAsync(80);

        await _handler.Handle(new ApprovePaymentCommand(payment.Id), CancellationToken.None);

        payment.Status.Should().Be(PaymentStatus.Approved);
        _trustScoreRepoMock.Verify(x => x.UpdateScoreAsync(tenantId, 85), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Payment_Not_Found()
    {
        _paymentRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Payment)null);
        
        Func<Task> act = async () => await _handler.Handle(new ApprovePaymentCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Payment not found.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Lease_Not_Found()
    {
        var payment = Payment.Create(Guid.NewGuid(), 1000, DateTime.UtcNow, "REF");
        _paymentRepoMock.Setup(x => x.GetByIdAsync(payment.Id)).ReturnsAsync(payment);
        _leaseRepoMock.Setup(x => x.GetByIdAsync(payment.LeaseId)).ReturnsAsync((Lease)null);

        Func<Task> act = async () => await _handler.Handle(new ApprovePaymentCommand(payment.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Lease associated with payment not found.");
    }
}
