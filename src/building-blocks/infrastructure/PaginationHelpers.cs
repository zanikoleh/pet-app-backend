namespace Infrastructure;

/// <summary>
/// Global query object to support filtering, sorting, and pagination.
/// Use this in API query parameters to standardize pagination across services.
/// </summary>
public class PaginatedQuery
{
    /// <summary>
    /// The page number (1-indexed).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// The size of each page.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Maximum allowed page size (security constraint).
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Property name to sort by (e.g., "Name", "CreatedAt").
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort order: "asc" or "desc".
    /// </summary>
    public string? SortOrder { get; set; } = "asc";

    /// <summary>
    /// Search filter text.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Validations for pagination parameters.
    /// </summary>
    public void Validate()
    {
        if (Page < 1)
            Page = 1;

        if (PageSize < 1)
            PageSize = 10;

        if (PageSize > MaxPageSize)
            PageSize = MaxPageSize;

        SortOrder = SortOrder?.ToLower() == "desc" ? "desc" : "asc";
    }

    /// <summary>
    /// Gets the number of items to skip.
    /// </summary>
    public int GetSkip() => (Page - 1) * PageSize;
}

/// <summary>
/// Generic paginated response object.
/// Use this to return paginated data in APIs.
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public PaginatedResponse()
    {
    }

    public PaginatedResponse(List<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PaginatedResponse<T> Create(List<T> items, int page, int pageSize, int totalCount)
    {
        return new(items, page, pageSize, totalCount);
    }
}
