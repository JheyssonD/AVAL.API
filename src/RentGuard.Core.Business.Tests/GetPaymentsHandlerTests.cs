using FluentAssertions;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.Payments.GetPayments;
using Xunit;
using Moq;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;

namespace RentGuard.Core.Business.Tests;

public class GetPaymentsHandlerTests
{
    private readonly Mock<IPaymentRepository> _repoMock;
    private readonly GetPaymentsHandler _handler;

    public GetPaymentsHandlerTests()
    {
        _repoMock = new Mock<IPaymentRepository>();
        _handler = new GetPaymentsHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_List_Of_Payments()
    {
        // Arrange
        var payments = new List<Payment> { 
            Payment.Create(Guid.NewGuid(), 1000, DateTime.UtcNow, "REF1"),
            Payment.Create(Guid.NewGuid(), 2000, DateTime.UtcNow, "REF2")
        };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(payments);

        // Act
        var result = await _handler.Handle(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }
}
