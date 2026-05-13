using Moq;
using Microsoft.Extensions.Localization;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using RentGuard.Core.Business.Modules.Payments.CreatePayment;
using RentGuard.Core.Business;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class CreatePaymentHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<ILeaseRepository> _leaseRepoMock;
    private readonly Mock<IStringLocalizer<SharedResources>> _localizerMock;
    private readonly CreatePaymentHandler _handler;

    public CreatePaymentHandlerTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _leaseRepoMock = new Mock<ILeaseRepository>();
        _localizerMock = new Mock<IStringLocalizer<SharedResources>>();
        _handler = new CreatePaymentHandler(_paymentRepoMock.Object, _leaseRepoMock.Object, _localizerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Throw_DomainException_When_Lease_Does_Not_Exist()
    {
        var leaseId = Guid.NewGuid();
        _leaseRepoMock.Setup(x => x.GetByIdAsync(leaseId)).ReturnsAsync((RentGuard.Core.Business.Modules.Leases.Domain.Lease)null);
        var command = new CreatePaymentCommand(leaseId, 1000, DateTime.UtcNow, "REF-ERROR");
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>().Where(e => e.Error.Code == DomainErrors.Leases.NotFound.Code);
    }
}
