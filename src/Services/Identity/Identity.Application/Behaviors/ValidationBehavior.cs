using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Identity.Application.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next);

            if (!_validators.Any())
            {
                return await next(cancellationToken).ConfigureAwait(false);
            }

            ValidationContext<TRequest> context = new(request);
            ValidationResult[] validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);

            List<ValidationFailure> failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new SharedKernel.Exceptions.ValidationException(
                    failures.Select(f => f.ErrorMessage).ToList());
            }

            return await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
