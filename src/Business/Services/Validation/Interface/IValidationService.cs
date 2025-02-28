namespace Business.Services.Validation.Interface;

public interface IValidationService
{
    Task<List<string>> ValidateAndGetErrorsAsync<T>(T obj);
    Task<string?>      ValidateAsync<T>(T             obj);
}
