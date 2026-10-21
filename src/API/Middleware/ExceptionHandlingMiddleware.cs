using System.Net;
using System.Text.Json;
using Application.DTOs.Common;
using Domain.Exceptions;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request execution: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException nf => (HttpStatusCode.NotFound, nf.Message, (IDictionary<string, string[]>?)null),
            ValidationException val => (HttpStatusCode.BadRequest, val.Message, val.Errors),
            UnauthorizedException unauth => (HttpStatusCode.Unauthorized, unauth.Message, null),
            KeyNotFoundException knf => (HttpStatusCode.NotFound, knf.Message, null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected server error occurred. Please try again later.", null)
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message, errors);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
