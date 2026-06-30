namespace LibraryClub.Api.Common;

public record PagedResult<T>(
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
