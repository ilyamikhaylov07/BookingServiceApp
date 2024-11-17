using Infrastructure.Logger;
using Serilog;
using SpecialistService.API.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

///
///  LOGGER
///
builder.Host.UseSerilog(SerilogExtensions.CreateLogger());
///
///  LOGGER
///

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
SwaggerSettings.AddLocker(builder);

var app = builder.Build();

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
