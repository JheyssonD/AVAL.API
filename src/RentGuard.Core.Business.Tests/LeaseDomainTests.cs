using FluentAssertions;
using RentGuard.Core.Business.Modules.Leases.Domain;
using Xunit;

namespace RentGuard.Core.Business.Tests;

public class LeaseDomainTests
{
    [Fact]
    public void Should_Create_Valid_Lease()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var tenantId = "tenant-123";
        var startDate = DateTime.UtcNow;
        var dueDay = 5;

        // Act
        var lease = Lease.Create(propertyId, tenantId, startDate, dueDay);

        // Assert
        lease.Should().NotBeNull();
        lease.IsActive.Should().BeTrue();
        lease.DueDayOfMonth.Should().Be(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void Should_Throw_Exception_When_DueDay_Is_Invalid(int invalidDay)
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var tenantId = "tenant-123";

        // Act
        Action act = () => Lease.Create(propertyId, tenantId, DateTime.UtcNow, invalidDay);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(2026, 2, 29, 2026, 3, 1)]  // Febrero 29 -> Marzo 1
    [InlineData(2026, 4, 31, 2026, 5, 1)]  // Abril 31 -> Mayo 1
    [InlineData(2026, 1, 15, 2026, 1, 15)] // Enero 15 -> Enero 15 (Normal)
    public void Should_Handle_Date_Rollover_Correctly(int year, int month, int dueDay, int expectedYear, int expectedMonth, int expectedDay)
    {
        // Arrange
        var lease = Lease.Create(Guid.NewGuid(), "tenant-1", DateTime.UtcNow, dueDay);

        // Act
        var dueDate = lease.GetDueDateForMonth(year, month);

        // Assert
        dueDate.Should().Be(new DateTime(expectedYear, expectedMonth, expectedDay));
    }

    [Fact]
    public void Property_Should_Track_Availability_Correctly()
    {
        // Arrange
        var property = Property.Create("Villa", "Main St 1", 1000, "USD", "landlord-1");

        // Act
        property.MarkAsRented();

        // Assert
        property.IsAvailable.Should().BeFalse();
    }
}
