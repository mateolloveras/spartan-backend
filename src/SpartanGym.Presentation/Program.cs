using SpartanGym.Application;
using SpartanGym.Infrastructure;
using SpartanGym.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation();

var app = builder.Build();

app.UsePathBase("/api");
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors(SpartanGym.Presentation.DependencyInjection.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();

public partial class Program;
