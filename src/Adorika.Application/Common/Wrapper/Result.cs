namespace Adorika.Application.Common.Wrapper;

public sealed record Result<T>
{
    public required bool IsSuccessful { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public List<ResultError>? Errors { get; init; }
    public List<string>? Warnings { get; init; }
    public PaginationMetadata? Pagination { get; init; }

    // Computed helper - zero memory overhead
    public bool HasData => Data is not null;

    // Factories - Extremely lean initialization
    public static Result<T> Success() => new() { IsSuccessful = true };
    public static Result<T> Success(T data) => new()
    {
        IsSuccessful = true,
        Data = data
    };
    public static Result<T> Failure() => new() { IsSuccessful = false };
    public static Result<T> Failure(ResultError error) => new()
    {
        IsSuccessful = false,
        Errors = [error]
    };
    public static Result<T> Failure(IEnumerable<ResultError> errors) => new()
    {
        IsSuccessful = false,
        Errors = [.. errors]
    };

    // Fluent API using 'with' expressions
    public Result<T> WithMessage(string message) => this with { Message = message };
    public Result<T> WithData(T data) => this with { Data = data };

    // Collection management using C# 12 spread for speed
    public Result<T> WithErrors(IEnumerable<ResultError> errors) => this with { Errors = [.. errors] };
    public Result<T> AppendErrors(IEnumerable<ResultError> errors) => this with { Errors = [.. Errors ?? [], .. errors] };
    public Result<T> AppendError(ResultError error) => this with { Errors = [.. Errors ?? [], error] };

    public Result<T> WithWarnings(IEnumerable<string> warnings) => this with { Warnings = [.. warnings] };
    public Result<T> AppendWarnings(IEnumerable<string> warnings) => this with { Warnings = [.. Warnings ?? [], .. warnings] };
    public Result<T> AppendWarning(string warning) => this with { Warnings = [.. Warnings ?? [], warning] };

    public Result<T> WithPagination(int pageNumber, int pageSize, int totalCount) =>
        this with { Pagination = new PaginationMetadata(pageNumber, pageSize, totalCount) };
}

public static class ResultExtensions
{
    public static Result<List<T>> AppendItem<T>(this Result<List<T>> result, T item) =>
        result with { Data = [.. result.Data ?? [], item] };

    public static Result<List<T>> AppendRange<T>(this Result<List<T>> result, IEnumerable<T> items) =>
        result with { Data = [.. result.Data ?? [], .. items] };
}

public record PaginationMetadata(int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public bool IsFirstPage => PageNumber == 1;
    public bool IsLastPage => PageNumber >= TotalPages;
}
