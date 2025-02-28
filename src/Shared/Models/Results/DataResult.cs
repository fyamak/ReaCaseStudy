using Shared.Constants;

namespace Shared.Models.Results;

public class DataResult<T> : Result where T : class?
{
    public T? Data { get; set; }

    public DataResult()
    {
    }

    protected DataResult(T? data, ResultStatus status, string? message) : base(status, message)
    {
        Data = data;
    }

    public static DataResult<T> Success(T data, string message = ResultMessages.Success)
    {
        return new DataResult<T>(data, ResultStatus.Success, message);
    }

    public static new DataResult<T> Error(string message = ResultMessages.Error)
    {
        return new DataResult<T>(null, ResultStatus.Error, message);
    }


    public static new DataResult<T> Invalid(string message = ResultMessages.Invalid)
    {
        return new DataResult<T>(null, ResultStatus.Invalid, message);
    }
}
