using RentGuard.Core.Business.Modules.Payments.Domain;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class PaymentDomainTests
{
    [Fact]
    public void Create_Payment_With_Zero_Amount_Should_Throw_Key()
    {
        Action act = () => Payment.Create(Guid.NewGuid(), 0, DateTime.UtcNow, "REF");
        act.Should().Throw<ArgumentException>().WithMessage("PaymentAmountInvalid");
    }
}
