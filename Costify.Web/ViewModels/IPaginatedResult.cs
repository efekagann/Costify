namespace Costify.Web.ViewModels;

public interface IPaginatedResult
{
    int PageIndex { get; }
    int TotalPages { get; }
    int TotalCount { get; }
    int PageSize { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}
