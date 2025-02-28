using Shared.Constants;

namespace Shared.Models.Results;

public class PagedResult<T> : DataResult<IEnumerable<T>?>
{
    public int PageNumber { get; set; }
    public int PageSize   { get; set; }
    public int TotalCount { get; set; }

    public PagedResult()
    {
    }

    public PagedResult(int pageNumber, int pageSize, int totalCount, IEnumerable<T>? data, string? message,
        ResultStatus       status)
        : base(data, status, message)
    {
        PageNumber = pageNumber;
        PageSize   = pageSize;
        TotalCount = totalCount;
    }

    public static PagedResult<T> Success(IEnumerable<T> data, int pageNumber, int pageSize, int totalCount, string message = ResultMessages.Success)
    {
        return new PagedResult<T>(pageNumber, pageSize, totalCount, data, message, ResultStatus.Success);
    }

    public static new PagedResult<T> Error(string message = ResultMessages.Error)
    {
        return new PagedResult<T>(0, 0, 0, null, message, ResultStatus.Error);
    }

    public static new PagedResult<T> Invalid(string message = ResultMessages.Invalid)
    {
        return new PagedResult<T>(0, 0, 0, null, message, ResultStatus.Invalid);
    }
}
