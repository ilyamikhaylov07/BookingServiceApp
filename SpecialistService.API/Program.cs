using Infrastructure.Logger;
using Infrastructure.RabbitMQ;
using MassTransit;
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

 

    // Регистрация consumer
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<UserRegisteredConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"]);
                h.Password(builder.Configuration["RabbitMQ:Password"]);
            });

            cfg.ConfigureEndpoints(context);
        });
        
    });

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