using System;
using System.Linq;
using System.Threading.Tasks;
using CustomerList.Models.ViewModels;
using CustomerList.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerList.Controllers;

public class CustomerController : Controller
{
    private readonly CustomerApiService _customerApiService;

    public CustomerController(CustomerApiService customerApiService)
    {
        _customerApiService = customerApiService;
    }

    public async Task<IActionResult> Index(
        string? searchTerm = null,
        string? type = null,
        string? status = null,
        string? sortBy = null,
        string? sortDirection = null,
        int page = 1,
        int pageSize = 10)
    {
        var allCustomers = await _customerApiService.GetCustomersAsync();
        var filtered = allCustomers.AsEnumerable();

        // ---- Search (name or code) ----
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            filtered = filtered.Where(c =>
                (c.FirstName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.SecondName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.ThirdName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.Code?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // ---- Filter by type ----
        if (string.Equals(type, "person", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(c => c.IsPerson);
        }
        else if (string.Equals(type, "company", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(c => !c.IsPerson);
        }

        // ---- Filter by status ----
        if (string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(c => c.IsActive);
        }
        else if (string.Equals(status, "inactive", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(c => !c.IsActive);
        }

        // ---- Sort ----
        var direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

        filtered = (sortBy?.ToLowerInvariant()) switch
        {
            "code" => direction == "asc"
                ? filtered.OrderBy(c => c.Code)
                : filtered.OrderByDescending(c => c.Code),
            "name" => direction == "asc"
                ? filtered.OrderBy(c => c.FirstName)
                : filtered.OrderByDescending(c => c.FirstName),
            "id" => direction == "asc"
                ? filtered.OrderBy(c => c.Id)
                : filtered.OrderByDescending(c => c.Id),
            _ => filtered.OrderBy(c => c.Id)
        };

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        // ---- Pagination ----
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var pagedCustomers = filteredList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(CustomerViewModel.FromCustomer)
            .ToList();

        var viewModel = new CustomerListViewModel
        {
            Customers = pagedCustomers,
            SearchTerm = searchTerm,
            TypeFilter = type,
            StatusFilter = status,
            SortBy = sortBy,
            SortDirection = direction,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return View(viewModel);
    }
}
