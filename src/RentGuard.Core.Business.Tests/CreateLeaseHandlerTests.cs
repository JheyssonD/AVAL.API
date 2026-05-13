using Moq;
using RentGuard.Core.Business.Modules.Leases.CreateLease;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class CreateLeaseHandlerTests
{
    private readonly Mock<IPropertyRepository> _propertyRepoMock;
    private readonly Mock<ILeaseRepository> _leaseRepoMock;
    private readonly CreateLeaseHandler _handler;

    public CreateLeaseHandlerTests()
    {
        _propertyRepoMock = new Mock<IPropertyRepository>();
        _leaseRepoMock = new Mock<ILeaseRepository>();
        _handler = new CreateLeaseHandler(_propertyRepoMock.Object, _leaseRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Lease_And_Mark_Property_As_Rented()
    {
        // Arrange
        var property = Property.Create("Villa", "Address", 1000, "USD", "landlord-1");
        _propertyRepoMock.Setup(x => x.GetByIdAsync(property.Id)).ReturnsAsync(property);
        
        var command = new CreateLeaseCommand(property.Id, "tenant-1", DateTime.UtcNow, 5);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        property.IsAvailable.Should().BeFalse();
        _leaseRepoMock.Verify(x => x.AddAsync(It.IsAny<Lease>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_If_Property_Not_Available()
    {
        // Arrange
        var property = Property.Create("Villa", "Address", 1000, "USD", "landlord-1");
        property.MarkAsRented(); // Ya est alquilada
        _propertyRepoMock.Setup(x => x.GetByIdAsync(property.Id)).ReturnsAsync(property);
        
        var command = new CreateLeaseCommand(property.Id, "tenant-1", DateTime.UtcNow, 5);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not available*");
    }
}
