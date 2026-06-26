using System;
using System.Linq;
using System.Threading.Tasks;
using CustomerList.Models;
using CustomerList.Models.ViewModels;
using CustomerList.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CustomerList.Controllers;

[Authorize(Roles = "Administrator,Manager,Employee")]
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
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var viewModel = new CustomerListViewModel
        {
            SearchTerm = searchTerm,
            TypeFilter = type,
            StatusFilter = status,
            SortBy = sortBy,
            PageNumber = page,
            PageSize = pageSize
        };

        try
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
            var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

            // ---- Guard: requested page beyond the available range ----
            // Rather than silently rendering an empty table, snap back to
            // the last valid page so the user always sees something.
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var pagedCustomers = filteredList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(CustomerViewModel.FromCustomer)
                .ToList();

            viewModel.Customers = pagedCustomers;
            viewModel.SortDirection = direction;
            viewModel.PageNumber = page;
            viewModel.TotalCount = totalCount;
        }
        catch (ApplicationException ex)
        {
            // Thrown deliberately by CustomerApiService for known failure
            // modes (network unreachable, unexpected API response shape).
            // The message is already written to be safe to show directly.
            viewModel.ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            // Anything unexpected -- don't leak internal details to the
            // page, just show a generic, honest message.
            viewModel.ErrorMessage = "Something went wrong while loading customers. Please try again.";
        }

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var customer = await _customerApiService.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }
        catch (ApplicationException ex)
        {
            ViewData["ErrorMessage"] = ex.Message;
            return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }
    }
}