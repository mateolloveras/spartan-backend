using MediatR;

namespace SpartanGym.Tests.Support;

public record ProbeCommand(Guid UserId, int PageSize) : IRequest<ProbeResult>;

public record ProbeResult(Guid UserId, int PageSize);

public class ProbeCommandHandler : IRequestHandler<ProbeCommand, ProbeResult>
{
    public Task<ProbeResult> Handle(ProbeCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ProbeResult(request.UserId, request.PageSize));
    }
}
