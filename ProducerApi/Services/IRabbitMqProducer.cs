using Shared.Models;

namespace ProducerApi.Services
{
    public interface IRabbitMqProducer
    {
        Task SendMessageAsync(MessageDto message);
    }
}
