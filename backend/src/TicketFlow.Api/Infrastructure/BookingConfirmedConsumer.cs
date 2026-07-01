using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Messages;

namespace TicketFlow.Api.Infrastructure;

public class BookingConfirmedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BookingConfirmedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private ServiceBusClient? _client;
    private ServiceBusProcessor? _processor;

    public BookingConfirmedConsumer(
        IConfiguration configuration,
        ILogger<BookingConfirmedConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionString = _configuration["ServiceBus:ConnectionString"]!;
        var queueName = _configuration["ServiceBus:BookingConfirmedQueue"] ?? "booking-confirmed";

        var clientOptions = new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpTcp
        };

        _client = new ServiceBusClient(connectionString, clientOptions);
        _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 2
        });

        _processor.ProcessMessageAsync += OnMessageReceivedAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { });

        await _processor.StopProcessingAsync();
    }

    private async Task OnMessageReceivedAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var confirmedEvent = JsonSerializer.Deserialize<BookingConfirmedEvent>(body);

            if (confirmedEvent is not null)
            {
                _logger.LogInformation(
                    "BookingConfirmed event received: BookingId={BookingId}, ReferenceCode={ReferenceCode}",
                    confirmedEvent.BookingId,
                    confirmedEvent.ReferenceCode);

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                var booking = await db.Bookings
                    .Include(b => b.Tickets)
                    .FirstOrDefaultAsync(b => b.Id == confirmedEvent.BookingId);

                if (booking is not null)
                {
                    _logger.LogInformation(
                        "Saga complete: Booking {BookingId} confirmed with {Count} ticket(s) booked. ReferenceCode={ReferenceCode}",
                        confirmedEvent.BookingId,
                        booking.Tickets.Count,
                        confirmedEvent.ReferenceCode);

                    // Future: send confirmation email, notify payment service, update analytics
                }
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing BookingConfirmed message. Abandoning.");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Service Bus error. Source={ErrorSource}, Namespace={Namespace}, EntityPath={EntityPath}",
            args.ErrorSource,
            args.FullyQualifiedNamespace,
            args.EntityPath);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _processor?.DisposeAsync().AsTask().Wait();
        _client?.DisposeAsync().AsTask().Wait();
        base.Dispose();
    }
}