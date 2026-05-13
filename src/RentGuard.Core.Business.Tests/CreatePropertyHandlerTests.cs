using Moq;
using RentGuard.Core.Business.Modules.Leases.CreateProperty;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using Xunit;

namespace RentGuard.Core.Business.Tests;

public class CreatePropertyHandlerTests
{
    private readonly Mock<IPropertyRepository> _repositoryMock;
    private readonly CreatePropertyHandler _handler;

    public CreatePropertyHandlerTests()
    {
        _repositoryMock = new Mock<IPropertyRepository>();
        _handler = new CreatePropertyHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Call_Repository_Add()
    {
        // Arrange
        var command = new CreatePropertyCommand("Beach House", "Malibu 1", 5000, "USD", "landlord-1");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Property>()), Times.Once);
    }
}
