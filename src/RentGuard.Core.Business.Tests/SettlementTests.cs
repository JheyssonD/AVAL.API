using Moq;
using RentGuard.Core.Business.Modules.Payments.ApprovePayment;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using RentGuard.Core.Business.Shared.Outbox;
using RentGuard.Core.Business.Shared;
using Microsoft.Extensions.Localization;
using Xunit;

namespace RentGuard.Core.Business.Tests;

public class SettlementTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ILeaseRepository> _leaseRepositoryMock;
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IStringLocalizer<SharedResources>> _localizerMock;
    private readonly ApprovePaymentHandler _handler;

    public SettlementTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _leaseRepositoryMock = new Mock<ILeaseRepository>();
        _outboxRepositoryMock = new Mock<IOutboxRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _localizerMock = new Mock<IStringLocalizer<SharedResources>>();

        _handler = new ApprovePaymentHandler(
            _paymentRepositoryMock.Object,
            _leaseRepositoryMock.Object,
            _outboxRepositoryMock.Object,
            _tenantContextMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task ApprovePayment_ShouldUpdateLeaseBalanceAndOutbox()
    {
        // Arrange
        var leaseId = Guid.NewGuid();
        var payment = Payment.Create(leaseId, 1200, DateTime.UtcNow, "REF");
        payment.Process(); // FSM: Must be in processing to approve

        // Lease with 1000 debt
        var lease = Lease.Create(Guid.NewGuid(), "res-1", DateTime.UtcNow, 5, 1000);
        // We set debt manually since we don't have a BillingEngine implemented yet
        var debtField = typeof(Lease).GetProperty("DebtBalance");
        debtField?.SetValue(lease, 1000m);

        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(payment.Id)).ReturnsAsync(payment);
        _leaseRepositoryMock.Setup(r => r.GetByIdForUpdateAsync(leaseId)).ReturnsAsync(lease);

        // Act
        await _handler.Handle(new ApprovePaymentCommand(payment.Id), CancellationToken.None);

        // Assert
        Assert.Equal(PaymentStatus.Approved, payment.Status);
        Assert.Equal(0, lease.DebtBalance);
        Assert.Equal(200, lease.CreditBalance); // 1200 - 1000 debt = 200 credit
        
        _paymentRepositoryMock.Verify(r => r.UpdateAsync(payment), Times.Once);
        _leaseRepositoryMock.Verify(r => r.UpdateAsync(lease), Times.Once);
        _outboxRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<OutboxMessage>()), Times.Once);
    }
}
