using RabbitMQ.Client;
using Shared.Models;
using System.Text;
using System.Text.Json;

namespace ProducerApi.Services
{
    public class RabbitMqProducer : IRabbitMqProducer
    {
        private readonly IConfiguration _configuration;

        public RabbitMqProducer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendMessageAsync(MessageDto message)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"]!,
                UserName = _configuration["RabbitMQ:UserName"]!,
                Password = _configuration["RabbitMQ:Password"]!
            };

            await using var connection = await factory.CreateConnectionAsync();

            await using var channel = await connection.CreateChannelAsync();

            var queueName = _configuration["RabbitMQ:QueueName"]!;

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var json = JsonSerializer.Serialize(message);

            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
        }
    }
}
