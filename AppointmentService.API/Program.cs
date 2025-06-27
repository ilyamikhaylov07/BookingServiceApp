using AppointmentService.API.Helpers;
using AppointmentService.API.Middleware;
using AppointmentService.API.Repositories;
using AppointmentService.API.Services;
using AppointmentService.API.Services.Interfaces;
using AppointmentService.API.Swagger;
using Infrastructure.Logger;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

///
///  LOGGER
///
builder.Host.UseSerilog(SerilogExtensions.CreateLogger(builder));
///
///  LOGGER
///

///
///  AUTHENTICATION
///
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Access", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });
///
///  AUTHENTICATION
///

/// 
/// CONSUMER
///
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SpecialistCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.ReceiveEndpoint("appointment-queue", e =>
        {
            e.ConfigureConsumer<SpecialistCreatedConsumer>(context);
        });
    });

});
/// 
/// CONSUMER
///

builder.Services.AddAuthorization();
builder.Services.AddDbContext<AppointmentDbContext>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<ISubscribeService, SubscribeService>();

SwaggerSettings.AddLocker(builder);


var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "{RequestMethod} {RequestPath} Ip: {ClientIp} StatusCode: {StatusCode}";
    options.EnrichDiagnosticContext = (diagContext, httpContext) =>
    {
        var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                httpContext.Connection.RemoteIpAddress?.ToString();

        diagContext.Set("ClientIp", ip);
        diagContext.Set("URI", httpContext.Request.Path);
        diagContext.Set("Method", httpContext.Request.Method);
        diagContext.Set("StatusCode", httpContext.Response.StatusCode);
    };
});

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader());

app.UseMiddleware<MiddlewareException>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { };



