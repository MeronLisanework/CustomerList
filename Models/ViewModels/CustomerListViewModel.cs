using System.Collections.Generic;

namespace CustomerList.Models.ViewModels;

// Wraps the customer list along with search, filter, sorting, and
// pagination state. This is the model the Index view binds to.
// Everything is strongly typed here -- no ViewBag is used anywhere,
// so the view and controller share a single typed contract.
public class CustomerListViewModel
{
    public List<CustomerViewModel> Customers { get; set; } = new();

    // Set when the API call failed -- the view shows this instead of
    // crashing the whole page with the default error screen.
    public string? ErrorMessage { get; set; }

    // Search
    public string? SearchTerm { get; set; }

    // Filters
    public string? TypeFilter { get; set; }   // "person" | "company" | null
    public string? StatusFilter { get; set; } // "active" | "inactive" | null

    // Sorting
    public string? SortBy { get; set; }        // "id" | "code" | "name"
    public string? SortDirection { get; set; } // "asc" | "desc"

    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)System.Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasFilters =>
        !string.IsNullOrEmpty(SearchTerm) ||
        !string.IsNullOrEmpty(TypeFilter) ||
        !string.IsNullOrEmpty(StatusFilter);
}