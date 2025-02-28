using Business.Services.Validation.Interface;
using FluentValidation;

namespace Business.Services.Validation;

public class ValidationService : IValidationService
{
    private readonly IEnumerable<IValidator> _validators;

    public ValidationService(IEnumerable<IValidator> validators)
    {
        _validators = validators;
    }

    public async Task<string?> ValidateAsync<T>(T obj)
    {
        var errors = await ValidateAndGetErrorsAsync(obj);

        return errors.Count > 0 ? string.Join(" ", errors) : null;
    }

    public async Task<List<string>> ValidateAndGetErrorsAsync<T>(T obj)
    {
        var validator = _validators.FirstOrDefault(v => v.CanValidateInstancesOfType(typeof(T)));

        if (validator == null)
        {
            throw new InvalidOperationException($"No validator found for type {typeof(T).Name}");
        }

        var context = new ValidationContext<T>(obj);

        var result = await validator.ValidateAsync(context);

        return result.Errors.Select(e => e.ErrorMessage.Replace("'", "")).ToList();
    }
}
