using System.Net;
using System.Text.Json;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Api.Middleware;

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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        var (status, code) = ex switch
        {
            EntityNotFoundException        => (HttpStatusCode.NotFound, "NOT_FOUND"),
            BusinessRuleViolationException b => (HttpStatusCode.UnprocessableEntity, b.RuleCode),
            FluentValidation.ValidationException => (HttpStatusCode.BadRequest, "VALIDATION_ERROR"),
            _                              => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR")
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception");

        ctx.Response.StatusCode = (int)status;
        ctx.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = "about:blank",
            title = ex.GetType().Name,
            status = (int)status,
            detail = ex.Message,
            code
        };

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
