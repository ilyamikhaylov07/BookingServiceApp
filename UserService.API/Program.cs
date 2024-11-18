using Infrastructure.Logger;
using Infrastructure.RabbitMQ;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using UserService.API.Repositories;
using UserService.API.Services;
using UserService.API.Swagger;

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

    builder.Services.AddMassTransitWithRabbitMQ(builder.Configuration);
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    SwaggerSettings.AddLocker(builder);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddCors();
    builder.Services.AddDbContext<UserDbContext>();

    /// 
    /// Аунтефикация
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
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<TokenManager>>();
                    logger.LogError(context.Exception, "Ошибка аутентификации токена");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<TokenManager>>();
                    logger.LogInformation("Токен успешно валидирован для пользователя: {User}", context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                }
            };
        })
        .AddJwtBearer("Refresh", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.FromMinutes(1),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = AuthOptions.GetSymSecurityKey(),
                ValidateIssuerSigningKey = true,
            };

        });

    /// 
    /// Аунтефикация
    ///

    builder.Services.AddAuthorization();

    /// 
    /// Зависимости TokenManager
    ///
    builder.Services.AddScoped(provider =>
    {
        return new TokenManager(
            AuthOptions.GetSymSecurityKey(),
            AuthOptions.ISSUER,
            AuthOptions.AUDIENCE
            );
    });
    /// 
    /// Зависимости TokenManager
    ///

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

