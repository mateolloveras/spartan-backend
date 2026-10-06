using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpartanGym.Application.Common;
using SpartanGym.Domain.Exceptions;

namespace SpartanGym.Tests.Support;

public enum ProbeStatus
{
    Confirmed,
    NoShow
}

public record ProbeItemDto(ProbeStatus Status, DateTimeOffset At);

public record ProbeValidateRequest(
    [property: JsonPropertyName("user_id")] Guid UserId,
    [property: JsonPropertyName("page_size")] int PageSize);

[ApiController]
[Route("__probe")]
public class ProbeController : ControllerBase
{
    public const string SecretMessage = "detalle-interno-secreto";

    private readonly IMediator _mediator;

    public ProbeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ProbeValidateRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ProbeCommand(request.UserId, request.PageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("throw/{kind}")]
    public IActionResult Throw(string kind)
    {
        throw kind switch
        {
            "not_found" => new NotFoundException("No se encontró el recurso de prueba."),
            "validation" => new ValidationException("Dato de prueba inválido."),
            "domain" => new DomainException("Regla de negocio de prueba violada."),
            "unauthorized" => new UnauthorizedException("Acceso de prueba no autorizado."),
            _ => new InvalidOperationException(SecretMessage)
        };
    }

    [HttpGet("paged")]
    public IActionResult Paged()
    {
        var items = new List<ProbeItemDto>
        {
            new(ProbeStatus.NoShow, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        };
        return Ok(new PagedResult<ProbeItemDto>(items, 1, 20, 1));
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = "admin")]
    public IActionResult AdminOnly()
    {
        return Ok();
    }
}
