using Azure.Messaging.ServiceBus;
using System.Text.Json;
using TicketFlow.Application.Common.Messages;

namespace TicketFlow.Api.Infrastructure;

public class BookingCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BookingCreatedConsumer> _logger;
    private ServiceBusClient? _client;
    private ServiceBusProcessor? _processor;

    public BookingCreatedConsumer(
        IConfiguration configuration,
        ILogger<BookingCreatedConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionString = _configuration["ServiceBus:ConnectionString"]!;
        var queueName = _configuration["ServiceBus:BookingCreatedQueue"] ?? "booking-created";

        var clientOptions = new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpTcp  // ✅ Correct for emulator
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

        // Keep running until stopped
        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { });

        await _processor.StopProcessingAsync();
    }

    private async Task OnMessageReceivedAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var bookingEvent = JsonSerializer.Deserialize<BookingCreatedEvent>(body);

            if (bookingEvent is not null)
            {
                _logger.LogInformation(
                    "BookingCreated event received: BookingId={BookingId}, " +
                    "ReferenceCode={ReferenceCode}, TotalAmount={TotalAmount}",
                    bookingEvent.BookingId,
                    bookingEvent.ReferenceCode,
                    bookingEvent.TotalAmount);

                // Phase 5: Log only — Phase 6 will trigger payment/notification here
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing BookingCreated message");
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