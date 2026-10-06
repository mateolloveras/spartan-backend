using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using SpartanGym.Tests.Support;

namespace SpartanGym.Tests.Presentation;

public class PipelineTests : IClassFixture<SpartanWebApplicationFactory>
{
    private readonly SpartanWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PipelineTests(SpartanWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    private static void AssertProblem(HttpResponseMessage response, JsonElement body, int status, string title)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(status, body.GetProperty("status").GetInt32());
        Assert.Equal(title, body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Health_SinToken_Devuelve200Healthy()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync(response);
        Assert.Equal("healthy", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task OpenApi_EnDevelopment_Devuelve200()
    {
        var response = await _client.GetAsync("/api/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OpenApi_EnProduction_Devuelve404()
    {
        var response = await _factory
            .WithWebHostBuilder(b => b.UseEnvironment("Production"))
            .CreateClient().GetAsync("/api/openapi/v1.json");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Validate_Invalido_Devuelve400ConErrorsSnakeCase()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/__probe/validate", new { user_id = Guid.Empty, page_size = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadAsync(response);
        AssertProblem(response, body, 400, "Datos inválidos");
        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("user_id", out _));
        Assert.True(errors.TryGetProperty("page_size", out _));
    }

    [Fact]
    public async Task Validate_Valido_Devuelve200()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/__probe/validate", new { user_id = Guid.NewGuid(), page_size = 10 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync(response);
        Assert.Equal(10, body.GetProperty("page_size").GetInt32());
    }

    [Theory]
    [InlineData("not_found", 404, "Recurso no encontrado")]
    [InlineData("validation", 400, "Datos inválidos")]
    [InlineData("unauthorized", 401, "No autorizado")]
    [InlineData("domain", 409, "Regla de negocio violada")]
    public async Task Throw_ExcepcionDeDominio_MapeaStatus(string kind, int status, string title)
    {
        var response = await _client.GetAsync($"/api/__probe/throw/{kind}");

        Assert.Equal(status, (int)response.StatusCode);
        var body = await ReadAsync(response);
        AssertProblem(response, body, status, title);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("detail").GetString()));
    }

    [Fact]
    public async Task Throw_Inesperada_Devuelve500SinFiltrarMensaje()
    {
        var response = await _client.GetAsync("/api/__probe/throw/unexpected");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(ProbeController.SecretMessage, text);
        var body = JsonDocument.Parse(text).RootElement;
        AssertProblem(response, body, 500, "Error interno");
        Assert.Equal(
            "Ocurrió un error inesperado. Intente nuevamente más tarde.",
            body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task AdminOnly_SinHeader_Devuelve401ConProblemDetails()
    {
        var response = await _client.GetAsync("/api/__probe/admin-only");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await ReadAsync(response);
        AssertProblem(response, body, 401, "No autorizado");
        Assert.Equal("Se requiere autenticación válida.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task AdminOnly_ConRolClient_Devuelve403ConProblemDetails()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/__probe/admin-only");
        request.Headers.Add(TestAuthHandler.RoleHeader, "client");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await ReadAsync(response);
        AssertProblem(response, body, 403, "Acceso denegado");
    }

    [Fact]
    public async Task AdminOnly_ConRolAdmin_Devuelve200()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/__probe/admin-only");
        request.Headers.Add(TestAuthHandler.RoleHeader, "admin");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RutaInexistente_Devuelve404ConProblemDetails()
    {
        var response = await _client.GetAsync("/api/no-existe");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await ReadAsync(response);
        AssertProblem(response, body, 404, "Recurso no encontrado");
        Assert.Equal("El recurso solicitado no existe.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Paged_SerializaSnakeCaseEnumYFechaIso()
    {
        var response = await _client.GetAsync("/api/__probe/paged");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync(response);
        Assert.Equal(1, body.GetProperty("page").GetInt32());
        Assert.Equal(20, body.GetProperty("page_size").GetInt32());
        Assert.Equal(1, body.GetProperty("total_count").GetInt32());
        var item = body.GetProperty("items")[0];
        Assert.Equal("no_show", item.GetProperty("status").GetString());
        Assert.Equal("2026-01-01T00:00:00+00:00", item.GetProperty("at").GetString());
    }

    [Fact]
    public async Task ErrorConOriginAngular_IncluyeHeaderCors()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/__probe/throw/domain");
        request.Headers.Add("Origin", "http://localhost:4200");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
