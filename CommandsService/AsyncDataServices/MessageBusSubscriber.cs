using System.Text;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices;

public class MessageBusSubscriber
    : BackgroundService
{
    private readonly IConfiguration config;
    private readonly IEventProcessor eventProcessor;
    private IConnection? connection;
    private IModel? channel;
    private string? queueName;

    public MessageBusSubscriber(
        IConfiguration config
        , IEventProcessor eventProcessor
    )
    {
        this.config = config;
        this.eventProcessor = eventProcessor;

        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = config["RabbitMQHost"]
            , Port = int.Parse(config["RabbitMQPort"])
        };
        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        channel.ExchangeDeclare(
            exchange: "trigger"
            , type: ExchangeType.Fanout);
        queueName = channel.QueueDeclare().QueueName;
        channel.QueueBind(
            queue: queueName
            , exchange: "trigger"
            , routingKey: "");

        Console.WriteLine("--> Listening on the Message Bus...");

        connection.ConnectionShutdown += RabbitMQConnectionShutDown;
    }

    private void RabbitMQConnectionShutDown(
        object? sender
        , ShutdownEventArgs e)
    {
        Console.WriteLine("--> Connection Shutdown");
    }

    public override void Dispose()
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(connection);
        if(channel.IsOpen)
        {
            channel.Close();
            connection.Close();
        }
        base.Dispose();
    }

    protected override Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (ModuleHandle, ea) =>
        {
            Console.WriteLine("--> Event Recived!");

            var body = ea.Body;
            var notoficationMsg = Encoding.UTF8.GetString(body.ToArray());

            eventProcessor.ProcessEvent(notoficationMsg);
        };

        channel.BasicConsume(
            queue: queueName
            , autoAck: true
            , consumer: consumer
            );
        
        return Task.CompletedTask;
    }
}