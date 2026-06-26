using System;
using System.Linq;
using System.Threading.Tasks;
using CustomerList.Models.ViewModels;
using CustomerList.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CustomerList.Controllers;
[Authorize(Roles = "Administrator,Manager")]
public class DashboardController : Controller
{
    private readonly CustomerApiService _customerApiService;

    public DashboardController(CustomerApiService customerApiService)
    {
        _customerApiService = customerApiService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new DashboardViewModel();

        try
        {
            // Same cached data source as the Customer list -- the dashboard
            // never makes an extra round trip to the external API.
            var customers = await _customerApiService.GetCustomersAsync();

            var now = DateTime.Now;

            viewModel.TotalCustomers = customers.Count;
            viewModel.ActiveCustomers = customers.Count(c => c.IsActive);
            viewModel.InactiveCustomers = viewModel.TotalCustomers - viewModel.ActiveCustomers;
            viewModel.NewCustomersThisMonth = customers.Count(c =>
                c.CreatedOn.HasValue &&
                c.CreatedOn.Value.Year == now.Year &&
                c.CreatedOn.Value.Month == now.Month);

            viewModel.IndividualCount = customers.Count(c => c.IsPerson);
            viewModel.OrganizationCount = viewModel.TotalCustomers - viewModel.IndividualCount;

            // ----- New customers per month (last 6 months, current month last) -----
            var monthlyTrend = new ChartSeries();
            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                monthlyTrend.Labels.Add(month.ToString("MMM yyyy"));
                monthlyTrend.Values.Add(customers.Count(c =>
                    c.CreatedOn.HasValue &&
                    c.CreatedOn.Value.Year == month.Year &&
                    c.CreatedOn.Value.Month == month.Month));
            }
            viewModel.MonthlyTrend = monthlyTrend;

            // ----- New customers per year -----
            // Grouping by year (rather than the Preference code, which has
            // too little variation in this dataset to produce a meaningful
            // multi-bar chart) gives a guaranteed multi-category breakdown
            // as long as records span more than one calendar year.
            var yearlyGroups = customers
                .Where(c => c.CreatedOn.HasValue)
                .GroupBy(c => c.CreatedOn!.Value.Year)
                .OrderBy(g => g.Key)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .ToList();

            viewModel.YearlyTrend = new ChartSeries
            {
                Labels = yearlyGroups.Select(x => x.Year.ToString()).ToList(),
                Values = yearlyGroups.Select(x => x.Count).ToList()
            };

            // ----- Recently added customers -----
            viewModel.RecentCustomers = customers
                .Where(c => c.CreatedOn.HasValue)
                .OrderByDescending(c => c.CreatedOn)
                .Take(5)
                .Select(c => new RecentCustomer
                {
                    Id = c.Id,
                    Code = c.Code,
                    DisplayName = c.FullName,
                    CreatedOn = c.CreatedOn
                })
                .ToList();

            // ----- Year-over-year growth -----
            viewModel.CurrentYearNewCustomers = customers.Count(c =>
                c.CreatedOn.HasValue && c.CreatedOn.Value.Year == now.Year);
            viewModel.PreviousYearNewCustomers = customers.Count(c =>
                c.CreatedOn.HasValue && c.CreatedOn.Value.Year == now.Year - 1);

            viewModel.YearOverYearGrowthPercent = viewModel.PreviousYearNewCustomers == 0
                ? null
                : Math.Round(
                    (viewModel.CurrentYearNewCustomers - viewModel.PreviousYearNewCustomers)
                        * 100.0 / viewModel.PreviousYearNewCustomers,
                    1);
        }
        catch (ApplicationException ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            viewModel.ErrorMessage = "Something went wrong while loading dashboard data. Please try again.";
        }

        return View(viewModel);
    }
}