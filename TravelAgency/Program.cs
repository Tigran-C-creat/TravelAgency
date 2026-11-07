using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using TravelAgency.Application.Commands.Employee;
using TravelAgency.Application.Commands.Login;
using TravelAgency.Application.Interfaces;
using TravelAgency.Application.Profiles;
using TravelAgency.Application.Queries.GetEmployee;
using TravelAgency.Domain.Interfaces;
using TravelAgency.Infrastructure.Configuration;
using TravelAgency.Infrastructure.Services;
using TravelAgency.Middleware;
using TravelAgency.Persistence;
using TravelAgency.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

// Настраиваем аутентификацию с использованием JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Настраиваем авторизацию для ролей чтения и записи
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireReadRole", policy =>
        policy.RequireRole(TravelAgency.Domain.Enums.TravelAgencyRole.Read));
    options.AddPolicy("RequireWriteRole", policy =>
        policy.RequireRole(TravelAgency.Domain.Enums.TravelAgencyRole.Write));
});

// Регистрируем контекст базы данных и настраиваем подключение к PostgreSQL
builder.Services.AddDbContext<TravelAgencyContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(LoginCommandHandler).Assembly,
                                    typeof(GetEmployeeQueryHandler).Assembly
      );
});


builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateEmployeeCommandHandler).Assembly));

builder.Services.AddScoped<IRepository, EfRepository>();

builder.Services.AddAutoMapper(typeof(EmployeeProfile));

var redisConnection = builder.Configuration["Redis:Connection"] ?? throw new InvalidOperationException("Redis connection string is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnection));

builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Введите JWT-токен в формате: Bearer {токен}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();