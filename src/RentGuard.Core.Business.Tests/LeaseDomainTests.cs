using RentGuard.Core.Business.Modules.Leases.Domain;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class LeaseDomainTests
{
    [Fact]
    public void Create_Lease_With_Invalid_DueDay_Should_Throw_Key()
    {
        Action act = () => Lease.Create(Guid.NewGuid(), "tenant-1", DateTime.UtcNow, 32);
        act.Should().Throw<ArgumentException>().WithMessage("LeaseDueDayInvalid");
    }
}
