using Moq;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using RentGuard.Core.Business.Modules.Leases.CreateLease;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class CreateLeaseHandlerTests
{
    private readonly Mock<ILeaseRepository> _leaseRepoMock;
    private readonly Mock<IPropertyRepository> _propertyRepoMock;
    private readonly CreateLeaseHandler _handler;

    public CreateLeaseHandlerTests()
    {
        _leaseRepoMock = new Mock<ILeaseRepository>();
        _propertyRepoMock = new Mock<IPropertyRepository>();
        _handler = new CreateLeaseHandler(_leaseRepoMock.Object, _propertyRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Property_Does_Not_Exist()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        _propertyRepoMock.Setup(x => x.GetByIdAsync(propertyId)).ReturnsAsync((Property)null);
        
        var command = new CreateLeaseCommand(propertyId, "tenant-1", DateTime.UtcNow, 5);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Property not found.");
    }
}
