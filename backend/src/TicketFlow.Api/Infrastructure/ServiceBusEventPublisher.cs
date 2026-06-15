using System.Text.Json;
using Azure.Messaging.ServiceBus;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Messages;

namespace TicketFlow.Api.Infrastructure;

public class ServiceBusEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public ServiceBusEventPublisher(IConfiguration configuration)
    {
        var connectionString = configuration["ServiceBus:ConnectionString"]!;
        var queueName = configuration["ServiceBus:BookingCreatedQueue"] ?? "booking-created";
        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(queueName);
    }

    public async Task PublishBookingCreatedAsync(BookingCreatedEvent @event, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(@event);
        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = "BookingCreated"
        };
        await _sender.SendMessageAsync(message, ct);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}