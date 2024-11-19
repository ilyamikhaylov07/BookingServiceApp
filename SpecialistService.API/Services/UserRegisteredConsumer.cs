using Infrastructure.Contracts;
using MassTransit;
using SpecialistService.API.Models;
using SpecialistService.API.Repositories;

namespace SpecialistService.API.Services
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly SpecialistDbContext _context;
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(SpecialistDbContext context, ILogger<UserRegisteredConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var message = context.Message;
            try
            {
                _logger.LogInformation("Received message for user {UserId} with role {Role}", message.UserId, message.Role);
                if (message.Role == "Specialist")
                {
                    var specialist = new Specialists
                    {
                        UserId = message.UserId
                    };

                    _context.Specialists.Add(specialist);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Specialist with UserId {UserId} added to the database", message.UserId);
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message for user {UserId}", message.UserId);
            }


        }
    }
}
