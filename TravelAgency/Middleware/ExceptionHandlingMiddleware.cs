using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StackExchange.Redis;
using System.Data.Common;
using TravelAgency.Shared.Exeptions;

namespace TravelAgency.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status404NotFound, "Not Found", ex.Message, "https://httpstatuses.com/404");
            }
            catch (LogicException ex)
            {
                await HandleExceptionAsync(context, (int)ex.StatusCode, "Business Logic Error", ex.Message, $"https://httpstatuses.com/{(int)ex.StatusCode}");
            }
            catch (RedisConnectionException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status503ServiceUnavailable, "Redis Connection Error", $"Ошибка подключения к Redis: {ex.Message}", "https://httpstatuses.com/503");
            }
            catch (PostgresException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status503ServiceUnavailable, "PostgreSQL Error", $"Ошибка PostgreSQL: {ex.Message}", "https://httpstatuses.com/503");
            }
            catch (DbException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status503ServiceUnavailable, "Database Error", $"Ошибка подключения к базе данных: {ex.Message}", "https://httpstatuses.com/503");
            }
            catch (Exception)
            {
                await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, "Internal Server Error", "Произошла непредвиденная ошибка. Попробуйте позже.", "https://httpstatuses.com/500");
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string title, string detail, string type)
        {
            var problem = new ProblemDetails
            {
                Type = type,
                Title = title,
                Status = statusCode,
                Detail = detail,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
