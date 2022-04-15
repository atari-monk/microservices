using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices;

public class MessageBusClient :
    IMessageBusClient
{
    private readonly IConfiguration config;
    private readonly IConnection? connection;
    private readonly IModel? channel;

    public MessageBusClient(
        IConfiguration config
    )
    {
        this.config = config;
        var factory = new ConnectionFactory()
        {
            HostName = config["RabbitMQHost"]
            , Port = int.Parse(config["RabbitMQPort"])
        };
        try
        {
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

            connection.ConnectionShutdown += RabbitMQConnectionShutdown;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
        }
    }

    private void RabbitMQConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        Console.WriteLine($"--> RabbitMQ Connection Shutdown");
    }

    public void PublishNewPlatform(
        PlatformPublishedDto platform)
    {
        var msg = JsonSerializer.Serialize(platform);
        ArgumentNullException.ThrowIfNull(connection);
        if(connection.IsOpen)
        {
            Console.WriteLine($"--> RabbitMQ Connection Open, sending message...");
            SendMessage(msg);
        }
        else
        {
            Console.WriteLine($"--> RabbitMQ Connection closed, not sending");
        }
    }

    private void SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        ArgumentNullException.ThrowIfNull(channel);
        channel.BasicPublish(
            exchange: "trigger"
            , routingKey: ""
            , basicProperties: null
            , body: body);
        Console.WriteLine($"--> We have sent {message}");
    }

    public void Dispose()
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(connection);
        if(channel.IsOpen)
        {
            channel.Close();
            connection.Close();
        }
        Console.WriteLine("MessageBus Disposed");
    }
}