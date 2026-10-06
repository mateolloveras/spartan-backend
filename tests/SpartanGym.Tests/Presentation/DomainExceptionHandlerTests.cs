using System.Text.Json;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SpartanGym.Domain.Exceptions;
using SpartanGym.Presentation.Middleware;
using DomainValidationException = SpartanGym.Domain.Exceptions.ValidationException;
using FluentValidationException = FluentValidation.ValidationException;

namespace SpartanGym.Tests.Presentation;

public class DomainExceptionHandlerTests
{
    private static async Task<(bool Handled, int Status, JsonElement Body)> RunAsync(Exception exception)
    {
        var handler = new DomainExceptionHandler(NullLogger<DomainExceptionHandler>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.Body.Position = 0;
        var body = context.Response.Body.Length == 0
            ? default
            : (await JsonDocument.ParseAsync(context.Response.Body)).RootElement.Clone();
        return (handled, context.Response.StatusCode, body);
    }

    public static IEnumerable<object[]> DomainCases()
    {
        yield return new object[] { new NotFoundException("No existe."), 404 };
        yield return new object[] { new DomainValidationException("Dato inválido."), 400 };
        yield return new object[] { new UnauthorizedException("No autorizado."), 401 };
        yield return new object[] { new DomainException("Regla violada."), 409 };
    }

    [Theory]
    [MemberData(nameof(DomainCases))]
    public async Task MapeaExcepcionDeDominioAStatusYDetail(Exception exception, int expectedStatus)
    {
        var (handled, status, body) = await RunAsync(exception);

        Assert.True(handled);
        Assert.Equal(expectedStatus, status);
        Assert.Equal(exception.Message, body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task FluentValidation_Devuelve400ConErrorsEnSnakeCase()
    {
        var exception = new FluentValidationException(new[]
        {
            new ValidationFailure("UserId", "El usuario es requerido."),
            new ValidationFailure("PageSize", "Mensaje uno."),
            new ValidationFailure("PageSize", "Mensaje dos.")
        });

        var (handled, status, body) = await RunAsync(exception);

        Assert.True(handled);
        Assert.Equal(400, status);
        var errors = body.GetProperty("errors");
        Assert.Equal("El usuario es requerido.", errors.GetProperty("user_id")[0].GetString());
        Assert.Equal(2, errors.GetProperty("page_size").GetArrayLength());
    }

    [Fact]
    public async Task ExcepcionInesperada_DevuelveFalse()
    {
        var (handled, _, _) = await RunAsync(new InvalidOperationException("bug"));
        Assert.False(handled);
    }
}
