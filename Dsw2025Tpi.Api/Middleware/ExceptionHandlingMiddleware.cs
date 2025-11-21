using Dsw2025Tpi.Application.Exceptions;
using System.Net;

namespace Dsw2025Tpi.Api.Middleware;

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
            _logger.LogError(ex, "Se capturo una excepcion no controlada: {ExceptionType}", ex.GetType().Name);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode status;
        string message;

        switch(exception)
        {
            case ArgumentException or
                FormatException or
                DuplicatedEntityException or
                EntityNotActiveException or
                InsufficientStockException:
                status = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            case EntityNotFoundException:
                status = HttpStatusCode.NotFound;
                message = exception.Message;
                break;

            case NoContentException:
                status = HttpStatusCode.NoContent;
                message = exception.Message;
                break;

            case UnauthorizedAccessException:
                status = HttpStatusCode.Unauthorized;
                message = "No tenes permiso para acceder a este recurso";
                break;

            default:
                status = HttpStatusCode.InternalServerError;
                message = "Ocurrió un error inesperado en el servidor.";
                break;
        }

        var result = System.Text.Json.JsonSerializer.Serialize(new { error = message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        return context.Response.WriteAsync(result);

    }
}
