using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class PaymentDomainTests
{
    [Fact]
    public void Create_Payment_With_Zero_Amount_Should_Throw_DomainException()
    {
        Action act = () => Payment.Create(Guid.NewGuid(), 0, DateTime.UtcNow, "REF");
        act.Should().Throw<DomainException>().Where(e => e.Error.Code == DomainErrors.Payments.AmountInvalid.Code);
    }
}
