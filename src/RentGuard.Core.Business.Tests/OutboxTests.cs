using Moq;
using RentGuard.Core.Business.Shared.Outbox;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class OutboxTests
{
    [Fact]
    public void CreateMessage_ShouldInitializeCorrectly()
    {
        var content = "{ \"PaymentId\": \"guid\" }";
        var type = "PaymentApprovedEvent";
        var tenantId = Guid.NewGuid();
        
        var message = new OutboxMessage(tenantId, type, content);

        message.Id.Should().NotBeEmpty();
        message.TenantId.Should().Be(tenantId);
        message.Type.Should().Be(type);
        message.Content.Should().Be(content);
        message.OccurredOnUtc.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        message.ProcessedOnUtc.Should().BeNull();
    }

    [Fact]
    public void MarkAsProcessed_ShouldUpdateTimestamp()
    {
        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}");
        
        message.MarkAsProcessed();

        message.ProcessedOnUtc.Should().NotBeNull();
        message.ProcessedOnUtc.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void SetError_ShouldStoreErrorMessage()
    {
        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}");
        var error = "Connection failed";
        
        message.SetError(error);

        message.Error.Should().Be(error);
    }
}
