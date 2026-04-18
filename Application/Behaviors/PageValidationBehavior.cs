using API.Application.Shared;
using MediatR;

namespace API.Application.Behaviors;

public class PageValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IPagedQuery {

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) {
        if (request.Page < 1)
            throw new ArgumentException("Page debe ser mayor o igual a 1.", nameof(request.Page));
        return await next();
    }
}
