using Moq;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using RentGuard.Core.Business.Modules.Payments.CreatePayment;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class CreatePaymentHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<ILeaseRepository> _leaseRepoMock;
    private readonly CreatePaymentHandler _handler;

    public CreatePaymentHandlerTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _leaseRepoMock = new Mock<ILeaseRepository>();
        _handler = new CreatePaymentHandler(_paymentRepoMock.Object, _leaseRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Lease_Does_Not_Exist()
    {
        // Arrange
        var leaseId = Guid.NewGuid();
        _leaseRepoMock.Setup(x => x.GetByIdAsync(leaseId)).ReturnsAsync((RentGuard.Core.Business.Modules.Leases.Domain.Lease)null);
        
        var command = new CreatePaymentCommand(leaseId, 1000, DateTime.UtcNow, "REF-ERROR");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Lease not found.");
    }
}
