using Moq;
using RentGuard.Core.Business.Modules.Payments.CreatePayment;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using Microsoft.Extensions.Localization;
using RentGuard.Core.Business.Modules.Payments.Domain;
using Xunit;

namespace RentGuard.Core.Business.Tests;

public class WebhookIdempotencyTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ILeaseRepository> _leaseRepositoryMock;
    private readonly Mock<IStringLocalizer<SharedResources>> _localizerMock;
    private readonly CreatePaymentHandler _handler;

    public WebhookIdempotencyTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _leaseRepositoryMock = new Mock<ILeaseRepository>();
        _localizerMock = new Mock<IStringLocalizer<SharedResources>>();
        
        _handler = new CreatePaymentHandler(
            _paymentRepositoryMock.Object,
            _leaseRepositoryMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task DuplicateMessageId_ShouldReturnSilently_WithoutAddingPayment()
    {
        // Arrange
        var messageId = "meta-msg-123";
        var existingPayment = Payment.Create(Guid.NewGuid(), 100, DateTime.UtcNow, "REF1", messageId);
        
        _paymentRepositoryMock
            .Setup(r => r.GetByExternalIdAsync(messageId))
            .ReturnsAsync(existingPayment);

        var command = new CreatePaymentCommand(Guid.NewGuid(), 100, DateTime.UtcNow, "REF1", messageId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Should NOT call LeaseRepository or AddAsync because of short-circuit
        _leaseRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task NewMessageId_ShouldProceedNormally()
    {
        // Arrange
        var messageId = "meta-msg-456";
        var leaseId = Guid.NewGuid();
        
        _paymentRepositoryMock
            .Setup(r => r.GetByExternalIdAsync(messageId))
            .ReturnsAsync((Payment?)null);

        _leaseRepositoryMock
            .Setup(r => r.GetByIdAsync(leaseId))
            .ReturnsAsync(new Mock<RentGuard.Core.Business.Modules.Leases.Domain.Lease>().Object);

        var command = new CreatePaymentCommand(leaseId, 100, DateTime.UtcNow, "REF2", messageId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
    }
}
