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
        
        var message = new OutboxMessage(type, content);

        message.Id.Should().NotBeEmpty();
        message.Type.Should().Be(type);
        message.Content.Should().Be(content);
        message.OccurredOnUtc.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        message.ProcessedOnUtc.Should().BeNull();
    }

    [Fact]
    public void MarkAsProcessed_ShouldUpdateTimestamp()
    {
        var message = new OutboxMessage("Test", "{}");
        
        message.MarkAsProcessed();

        message.ProcessedOnUtc.Should().NotBeNull();
        message.ProcessedOnUtc.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void SetError_ShouldStoreErrorMessage()
    {
        var message = new OutboxMessage("Test", "{}");
        var error = "Connection failed";
        
        message.SetError(error);

        message.Error.Should().Be(error);
    }
}
