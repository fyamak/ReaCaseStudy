using System.Text.Json.Serialization;
using Shared.Constants;

namespace Shared.Models.Results;

public class Result
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ResultStatus Status { get; set; }

    public string? Message { get; set; }

    public Result()
    {
    }

    protected Result(ResultStatus status, string? message)
    {
        Status  = status;
        Message = message;
    }

    public static Result Success(string message = ResultMessages.Success)
    {
        return new Result(ResultStatus.Success, message);
    }

    public static Result Error(string message = ResultMessages.Error)
    {
        return new Result(ResultStatus.Error, message);
    }

    public static Result Invalid(string message = ResultMessages.Invalid)
    {
        return new Result(ResultStatus.Invalid, message);
    }
}

public enum ResultStatus
{
    Success = 1,
    Error   = 2,
    Invalid = 3
}
