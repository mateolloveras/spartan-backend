using FluentValidation;
using FluentValidation.Results;
using SpartanGym.Application.Common.Behaviors;
using SpartanGym.Tests.Support;

namespace SpartanGym.Tests.Application;

public class ValidationBehaviorTests
{
    private static readonly ProbeCommand Valid = new(Guid.NewGuid(), 10);
    private static readonly ProbeCommand Invalid = new(Guid.Empty, 0);

    private static async Task<(ProbeResult Result, bool NextCalled)> RunAsync(
        IEnumerable<IValidator<ProbeCommand>> validators, ProbeCommand command)
    {
        var behavior = new ValidationBehavior<ProbeCommand, ProbeResult>(validators);
        var nextCalled = false;
        var result = await behavior.Handle(
            command,
            _ =>
            {
                nextCalled = true;
                return Task.FromResult(new ProbeResult(command.UserId, command.PageSize));
            },
            CancellationToken.None);
        return (result, nextCalled);
    }

    [Fact]
    public async Task SinValidators_LlamaANext()
    {
        var (_, nextCalled) = await RunAsync(Array.Empty<IValidator<ProbeCommand>>(), Invalid);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task ValidatorsValidos_LlamaANext()
    {
        var (result, nextCalled) = await RunAsync(new[] { new ProbeCommandValidator() }, Valid);
        Assert.True(nextCalled);
        Assert.Equal(Valid.UserId, result.UserId);
    }

    [Fact]
    public async Task ConFalla_LanzaValidationExceptionYNoLlamaANext()
    {
        var nextCalled = false;
        var behavior = new ValidationBehavior<ProbeCommand, ProbeResult>(new[] { new ProbeCommandValidator() });

        var ex = await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            Invalid,
            _ =>
            {
                nextCalled = true;
                return Task.FromResult(new ProbeResult(Guid.Empty, 0));
            },
            CancellationToken.None));

        Assert.False(nextCalled);
        Assert.Contains(ex.Errors, (ValidationFailure e) => e.PropertyName == "UserId");
        Assert.Contains(ex.Errors, (ValidationFailure e) => e.PropertyName == "PageSize");
    }
}
