using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class LeaseDomainTests
{
    [Fact]
    public void Create_Lease_With_Invalid_DueDay_Should_Throw_DomainException()
    {
        Action act = () => Lease.Create(Guid.NewGuid(), "tenant-1", DateTime.UtcNow, 32);
        act.Should().Throw<DomainException>().Where(e => e.Error.Code == DomainErrors.Leases.DueDayInvalid.Code);
    }
}
