namespace LibraryClub.Api.DTOs;

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages
    {
        get
        {
            if (PageSize <= 0 || TotalCount == 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(TotalCount / (double)PageSize);
        }
    }
}
