using System.Reflection;
using SpartanGym.Domain.Exceptions;

namespace SpartanGym.Tests.Architecture;

public class LayerDependencyTests
{
    private static readonly string[] DomainForbidden =
    [
        "SpartanGym.", "Microsoft.EntityFrameworkCore", "Npgsql", "Microsoft.AspNetCore", "MediatR", "FluentValidation"
    ];

    private static List<string> ReferencedNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToList();

    [Fact]
    public void Domain_NoReferenciaOtrasCapasNiFrameworks()
    {
        var referenced = ReferencedNames(typeof(DomainException).Assembly);

        foreach (var forbidden in DomainForbidden)
            Assert.DoesNotContain(referenced, n => n.StartsWith(forbidden, StringComparison.Ordinal));
    }

    [Fact]
    public void Application_NoReferenciaInfrastructureNiPresentation()
    {
        var referenced = ReferencedNames(typeof(SpartanGym.Application.DependencyInjection).Assembly);

        Assert.DoesNotContain("SpartanGym.Infrastructure", referenced);
        Assert.DoesNotContain("SpartanGym.Presentation", referenced);
    }
}
