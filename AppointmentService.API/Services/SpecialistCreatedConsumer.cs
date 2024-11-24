using AppointmentService.API.Models;
using AppointmentService.API.Repositories;
using Infrastructure.Contracts;
using MassTransit;

namespace AppointmentService.API.Services
{
    public class SpecialistCreatedConsumer : IConsumer<SpecialistCreatedEvent>
    {
        private readonly AppointmentDbContext _context;
        private readonly ILogger<SpecialistCreatedConsumer> _logger;

        public SpecialistCreatedConsumer(AppointmentDbContext context, ILogger<SpecialistCreatedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<SpecialistCreatedEvent> context)
        {
            var message = context.Message;
            try
            {
                _logger.LogInformation("Received message for user {UserId} and specialist {SpecialistId}", message.UserId, message.SpecialistId);

                var specialist = new Schedules
                {
                    SpecialistId = message.SpecialistId,
                    UserId = message.UserId,
                };

                _context.Schedules.Add(specialist);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Specialist with UserId {UserId} added to the database schedules", message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message for user {UserId}", message.UserId);
            }


        }
    }
}
