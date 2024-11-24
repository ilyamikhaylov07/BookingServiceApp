using Infrastructure.Logger;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SpecialistService.API.Repositories;
using SpecialistService.API.Services;
using SpecialistService.API.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
try
{
    ///
    ///  LOGGER
    ///
    builder.Host.UseSerilog(SerilogExtensions.CreateLogger());
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

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader());

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложению не удаётся запуститься из-за критической ошибки");
}
finally
{
    Log.CloseAndFlush();
}