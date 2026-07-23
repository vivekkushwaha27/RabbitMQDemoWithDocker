using Microsoft.AspNetCore.Mvc;
using ProducerApi.Models;
using ProducerApi.Services;
using Shared.Models;

namespace ProducerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IRabbitMqProducer _rabbitMqProducer;

    public MessagesController(
        IRabbitMqProducer rabbitMqProducer)
    {
        _rabbitMqProducer = rabbitMqProducer;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                Message = "Validation failed.",
                Errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                )
            });
        }

        var message = new MessageDto
        {
            Id = Guid.NewGuid(),
            Text = request.Message,
            CreatedAt = DateTime.UtcNow
        };

        await _rabbitMqProducer.SendMessageAsync(message);

        return Ok(new
        {
            Message = "Message sent to RabbitMQ successfully.",
            Data = message
        });
    }
}