using Moq;
using Microsoft.Extensions.Localization;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using RentGuard.Core.Business.Modules.TrustScore.Domain.Repositories;
using RentGuard.Core.Business.Modules.Payments.ApprovePayment;
using RentGuard.Core.Business.Shared;
using RentGuard.Core.Business.Shared.Outbox;
using RentGuard.Core.Business;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class ApprovePaymentHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<ILeaseRepository> _leaseRepoMock;
    private readonly Mock<IOutboxRepository> _outboxRepoMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IStringLocalizer<SharedResources>> _localizerMock;
    private readonly ApprovePaymentHandler _handler;

    public ApprovePaymentHandlerTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _leaseRepoMock = new Mock<ILeaseRepository>();
        _outboxRepoMock = new Mock<IOutboxRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _localizerMock = new Mock<IStringLocalizer<SharedResources>>();
        _handler = new ApprovePaymentHandler(
            _paymentRepoMock.Object, 
            _leaseRepoMock.Object, 
            _outboxRepoMock.Object, 
            _tenantContextMock.Object, 
            _localizerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Approve_Payment_And_Publish_Outbox_Message()
    {
        // Arrange
        var leaseId = Guid.NewGuid();
        var residentId = "tenant-123";
        var payment = Payment.Create(leaseId, 1000, DateTime.UtcNow, "REF");
        var lease = Lease.Create(Guid.NewGuid(), "resident-123", DateTime.UtcNow, 5, 1000m);
        
        _paymentRepoMock.Setup(x => x.GetByIdAsync(payment.Id)).ReturnsAsync(payment);
        _leaseRepoMock.Setup(x => x.GetByIdAsync(leaseId)).ReturnsAsync(lease);
        _tenantContextMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        await _handler.Handle(new ApprovePaymentCommand(payment.Id), CancellationToken.None);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Approved);
        _outboxRepoMock.Verify(x => x.SaveAsync(It.IsAny<OutboxMessage>()), Times.Once);
        _paymentRepoMock.Verify(x => x.UpdateAsync(payment), Times.Once);
    }
}

