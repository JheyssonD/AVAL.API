using RentGuard.Contracts.Modules.Payments;
using RentGuard.Core.Business.Modules.Payments.CreatePayment;
using FluentAssertions;
using Xunit;

namespace RentGuard.API.Tests;

public class PaymentIntegrationTests
{
    private readonly CreatePaymentHandler _handler = new();

    [Fact]
    public async Task CreatePayment_ShouldReturnReceivedStatus()
    {
        var req = new CreatePaymentRequest(
            Amount: 1500.50m,
            Currency: "USD",
            Reference: "RENT-MAY-2026",
            TenantId: "tenant-abc",
            PropertyId: "prop-123",
            PaymentDate: DateTime.UtcNow
        );

        var response = await _handler.Handle(req);

        response.Should().NotBeNull();
        response.Status.Should().Be("Received");
        response.Id.Should().NotBeEmpty();
    }
}
