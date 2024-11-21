using AppointmentService.API.Repositories;
using AppointmentService.API.Services;
using AppointmentService.API.Swagger;
using Infrastructure.Logger;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
try
{
    // Add services to the container.

    ///
    ///  LOGGER
    ///
    builder.Host.UseSerilog(SerilogExtensions.CreateLogger());
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


    SwaggerSettings.AddLocker(builder);
    var app = builder.Build();

    app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader());

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
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложению не удаётся запуститься из-за критической ошибки");
}
finally
{
    Log.CloseAndFlush();
}

