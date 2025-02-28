using Business.Mediator.Behaviours.Requests;
using Business.Services.Validation.Interface;
using MediatR;
using Shared.Models.Results;

namespace Business.Mediator.Behaviours;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IRequestToValidate where TResponse : Result, new()
{
    private readonly IValidationService _validationService;

    public ValidationBehavior(IValidationService validationService)
    {
        _validationService = validationService;
    }

    public async Task<TResponse> Handle(
        TRequest                          request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken                 cancellationToken)
    {
        var validationError = await _validationService.ValidateAsync(request);

        if (string.IsNullOrEmpty(validationError))
            return await next();

        return new TResponse { Message = validationError, Status = ResultStatus.Invalid };
    }
}
