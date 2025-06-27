using AppointmentService.API.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AppointmentService.Tests.InfrastructureTestContainer
{
    public class BaseIntegrationTest : IClassFixture<AppointmentWebApplicationFactory>
    {
        private readonly IServiceScope _scope;
        protected readonly AppointmentDbContext Dbcontext;
        protected readonly HttpClient Client;

        protected BaseIntegrationTest(AppointmentWebApplicationFactory factory)
        {
            _scope = factory.Services.CreateScope();
            Dbcontext = _scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
            Client = factory.CreateClient();
        }
    }
}
