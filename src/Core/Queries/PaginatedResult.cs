namespace Core.Queries;

public class PaginatedResult<T>
{
    public required int PageSize { get; init; }

    public required int PageNumber { get; init; }

    public required int TotalRecords { get; init; }

    public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);

    public required IEnumerable<T> Items { get; init; }
}
