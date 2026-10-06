using SpartanGym.Presentation.Middleware;
using SpartanGym.Presentation.Serialization;

namespace SpartanGym.Presentation;

public static class DependencyInjection
{
    public const string CorsPolicy = "Angular";

    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(o => JsonConventions.Apply(o.JsonSerializerOptions));

        services.ConfigureHttpJsonOptions(o => JsonConventions.Apply(o.SerializerOptions));

        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddProblemDetails(o => o.CustomizeProblemDetails = ProblemDetailsDefaults.Customize);
        services.AddOpenApi();

        services.AddCors(o => o.AddPolicy(CorsPolicy, policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()));

        services.AddAuthentication();
        services.AddAuthorization();

        return services;
    }
}
