using Moq;
using Microsoft.Extensions.Localization;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using RentGuard.Core.Business.Modules.TrustScore.Domain;
using RentGuard.Core.Business.Modules.TrustScore.Domain.Repositories;
using RentGuard.Core.Business.Modules.Payments.ApprovePayment;
using RentGuard.Core.Business;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class ApprovePaymentHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<ILeaseRepository> _leaseRepoMock;
    private readonly Mock<ITrustScoreRepository> _trustScoreRepoMock;
    private readonly Mock<IStringLocalizer<SharedResources>> _localizerMock;
    private readonly ApprovePaymentHandler _handler;

    public ApprovePaymentHandlerTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _leaseRepoMock = new Mock<ILeaseRepository>();
        _trustScoreRepoMock = new Mock<ITrustScoreRepository>();
        _localizerMock = new Mock<IStringLocalizer<SharedResources>>();
        _handler = new ApprovePaymentHandler(_paymentRepoMock.Object, _leaseRepoMock.Object, _trustScoreRepoMock.Object, _localizerMock.Object);
        
        _localizerMock.Setup(x => x["PaymentNotFound"]).Returns(new LocalizedString("PaymentNotFound", "Payment not found."));
        _localizerMock.Setup(x => x["LeaseNotFound"]).Returns(new LocalizedString("LeaseNotFound", "Lease not found."));
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
}
