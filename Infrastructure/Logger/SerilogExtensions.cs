using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Infrastructure.Logger
{
    public static class SerilogExtensions
    {
        public static ILogger CreateLogger(WebApplicationBuilder builder)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
        }
    }
}
