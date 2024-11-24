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
        private readonly IPublishEndpoint _publishEndpoint;

        public UserRegisteredConsumer(SpecialistDbContext context, ILogger<UserRegisteredConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
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

                    var specialistCreatedEvent = new SpecialistCreatedEvent
                    {
                        SpecialistId = specialist.Id,
                        UserId = message.UserId
                    };

                    await _publishEndpoint.Publish(specialistCreatedEvent);
                    _logger.LogInformation("Published SpecialistCreatedEvent for SpecialistId {SpecialistId}", specialist.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message for user {UserId}", message.UserId);
            }


        }
    }
}
