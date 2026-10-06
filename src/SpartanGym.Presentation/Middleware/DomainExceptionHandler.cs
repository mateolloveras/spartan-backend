using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SpartanGym.Domain.Exceptions;
using FluentValidationException = FluentValidation.ValidationException;

namespace SpartanGym.Presentation.Middleware;

public class DomainExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DomainExceptionHandler> _logger;

    public DomainExceptionHandler(ILogger<DomainExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            FluentValidationException ex => ToValidationProblem(ex),
            NotFoundException => Build(StatusCodes.Status404NotFound, "Recurso no encontrado", exception),
            ValidationException => Build(StatusCodes.Status400BadRequest, "Datos inválidos", exception),
            UnauthorizedException => Build(StatusCodes.Status401Unauthorized, "No autorizado", exception),
            DomainException => Build(StatusCodes.Status409Conflict, "Regla de negocio violada", exception),
            _ => null
        };

        if (problem is null)
            return false;

        _logger.LogWarning(exception, "Domain exception: {Message}", exception.Message);

        httpContext.Response.StatusCode = problem.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(
            problem, problem.GetType(), options: null, contentType: "application/problem+json", cancellationToken);

        return true;
    }

    private static ProblemDetails Build(int status, string title, Exception exception) =>
        new() { Status = status, Title = title, Detail = exception.Message };

    private static ValidationProblemDetails ToValidationProblem(FluentValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => JsonNamingPolicy.SnakeCaseLower.ConvertName(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Datos inválidos",
            Detail = "Uno o más campos no son válidos."
        };
    }
}
