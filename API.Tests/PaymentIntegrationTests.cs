using Moq;
using Microsoft.Extensions.Localization;
using RentGuard.Core.Business;
using RentGuard.Core.Business.Modules.Payments.CreatePayment;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using RentGuard.Core.Business.Modules.Leases.Domain;
using FluentAssertions;
using Xunit;

namespace RentGuard.API.Tests;

public class PaymentIntegrationTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
    private readonly Mock<ILeaseRepository> _leaseRepoMock = new();
    private readonly Mock<IStringLocalizer<SharedResources>> _localizerMock = new();
    private readonly CreatePaymentHandler _handler;

    public PaymentIntegrationTests()
    {
        _handler = new CreatePaymentHandler(_paymentRepoMock.Object, _leaseRepoMock.Object, _localizerMock.Object);
        _localizerMock.Setup(x => x["LeaseNotFound"]).Returns(new LocalizedString("LeaseNotFound", "Lease not found."));
    }

    [Fact]
    public async Task CreatePayment_Should_Succeed_When_Lease_Exists()
    {
        var leaseId = Guid.NewGuid();
        // Usar la Factory Create en lugar de constructor vaco
        var lease = Lease.Create(Guid.NewGuid(), "tenant-1", DateTime.UtcNow, 5);
        _leaseRepoMock.Setup(x => x.GetByIdAsync(leaseId)).ReturnsAsync(lease);
        
        var command = new CreatePaymentCommand(leaseId, 1500.50m, DateTime.UtcNow, "RENT-MAY-2026");

        await _handler.Handle(command, CancellationToken.None);

        _paymentRepoMock.Verify(x => x.AddAsync(It.IsAny<RentGuard.Core.Business.Modules.Payments.Domain.Payment>()), Times.Once);
    }
}
