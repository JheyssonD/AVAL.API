using System.Text.Json;
using RentGuard.Core.Business.Shared.Outbox;

namespace RentGuard.Presentation.API.Infrastructure.BackgroundServices;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages.");
            }

            await Task.Delay(5000, stoppingToken); // Poll every 5 seconds
        }
    }

    private async Task ProcessMessagesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        
        var messages = await repository.GetUnprocessedMessagesAsync(20);

        foreach (var message in messages)
        {
            _logger.LogInformation("Processing message {MessageId} of type {MessageType}", message.Id, message.Type);

            try
            {
                // In a real scenario, use MediatR or a dynamic dispatcher.
                // For MVP, we handle known events.
                if (message.Type == "PaymentApprovedEvent")
                {
                    // Logic to update Trust Score or notify
                }

                message.MarkAsProcessed();
                await repository.UpdateAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message {MessageId}", message.Id);
                message.SetError(ex.Message);
                await repository.UpdateAsync(message);
            }
        }
    }
}
