using MassTransit;
using SpecialistService.API.Contracts;
using System.IO;

namespace SpecialistService.API.Services
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var message = context.Message;
            string path = Directory.GetCurrentDirectory();
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                await writer.WriteLineAsync($"User Registered: {message.UserId}, {message.Email}, {message.Role}");
            }
            await Task.CompletedTask;
        }
    }
}
