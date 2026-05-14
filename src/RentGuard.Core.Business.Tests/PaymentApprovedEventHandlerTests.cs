using Moq;
using RentGuard.Core.Business.Modules.TrustScore.Domain;
using RentGuard.Core.Business.Modules.TrustScore.Domain.Repositories;
using RentGuard.Core.Business.Modules.Payments.Domain.Events;
using RentGuard.Core.Business.Modules.TrustScore.Handlers;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class PaymentApprovedEventHandlerTests
{
    private readonly Mock<ITrustScoreRepository> _trustScoreRepoMock;
    private readonly PaymentApprovedEventHandler _handler;

    public PaymentApprovedEventHandlerTests()
    {
        _trustScoreRepoMock = new Mock<ITrustScoreRepository>();
        _handler = new PaymentApprovedEventHandler(_trustScoreRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_UpdateScore_And_RecordSnapshot()
    {
        // Arrange
        var @event = new PaymentApprovedEvent(Guid.NewGuid(), Guid.NewGuid(), 1000m, "resident-1");
        _trustScoreRepoMock.Setup(x => x.GetCurrentScoreAsync("resident-1")).ReturnsAsync(800);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        _trustScoreRepoMock.Verify(x => x.AddHistoryAsync(It.IsAny<TrustScoreHistory>()), Times.Once);
        _trustScoreRepoMock.Verify(x => x.AddSnapshotAsync(It.IsAny<TrustScoreSnapshot>()), Times.Once);
    }
}
