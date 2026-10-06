using FluentValidation;

namespace SpartanGym.Tests.Support;

public class ProbeCommandValidator : AbstractValidator<ProbeCommand>
{
    public ProbeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El usuario es requerido.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100.");
    }
}
