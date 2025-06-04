using Infrastructure.Logger;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SpecialistService.API.Helpers;
using SpecialistService.API.Middleware;
using SpecialistService.API.Repositories;
using SpecialistService.API.Services;
using SpecialistService.API.Services.Interfaces;
using SpecialistService.API.Swagger;

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
/// CONSUMER
///
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredConsumer>();
    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.ReceiveEndpoint("specialist-queue", e =>
        {
            e.Bind("UserRegistered");
            e.ConfigureConsumer<UserRegisteredConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });

});
/// 
/// CONSUMER
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

builder.Services.AddAuthorization();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
SwaggerSettings.AddLocker(builder);
builder.Services.AddCors();
builder.Services.AddDbContext<SpecialistDbContext>();

builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ISkillService, SkillService>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<MiddlewareException>();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
