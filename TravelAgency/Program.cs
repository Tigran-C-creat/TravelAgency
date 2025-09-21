using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
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
builder.Services.AddControllers();

// Настройка аутентификации (например, JWT)
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

// Добавление авторизации
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireReadRole", policy =>
        policy.RequireRole(TravelAgency.Domain.Enums.TravelAgencyRole.Read));
    options.AddPolicy("RequireWriteRole", policy =>
        policy.RequireRole(TravelAgency.Domain.Enums.TravelAgencyRole.Write));
});

// Регистрируем контекст базы данных и указываем, что используем PostgreSQL
builder.Services.AddDbContext<TravelAgencyContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(LoginCommandHandler).Assembly,
                                    typeof(GetEmployeeQueryHandler).Assembly
      );
});

builder.Services.AddScoped<IRepository, EfRepository>();

builder.Services.AddAutoMapper(typeof(EmployeeProfile));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Введите токен JWT с префиксом Bearer",
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

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();