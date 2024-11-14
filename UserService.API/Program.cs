using Infrastructure.Logger;
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
                ClockSkew = TimeSpan.FromHours(2),
                ValidateIssuer = true,
                ValidIssuer = AuthOptions.ISSUER,
                ValidateAudience = true,
                ValidAudience = AuthOptions.AUDIENCE,
                ValidateLifetime = true,
                IssuerSigningKey = AuthOptions.GetSymSecurityKey(),
                ValidateIssuerSigningKey = true,
            };
        })
        .AddJwtBearer("Refresh", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.FromDays(7),
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
    builder.Services.AddSingleton(AuthOptions.GetSymSecurityKey());
    builder.Services.AddSingleton(AuthOptions.ISSUER);
    builder.Services.AddSingleton(AuthOptions.AUDIENCE);
    builder.Services.AddScoped<TokenManager>();
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
catch(Exception ex)
{
    Log.Fatal(ex, "Приложению не удаётся запуститься из-за критической ошибки");
}
finally
{
    Log.CloseAndFlush();
}

