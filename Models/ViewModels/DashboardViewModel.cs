namespace CustomerList.Models.ViewModels;

// Drives the Dashboard/Index page.
public class DashboardViewModel
{
    public string? ErrorMessage { get; set; }

    // ----- KPI cards -----
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }

    public double ActivePercentage =>
        TotalCustomers == 0 ? 0 : System.Math.Round(ActiveCustomers * 100.0 / TotalCustomers, 1);

    // ----- Composition (feeds the Customer Type doughnut) -----
    public int IndividualCount { get; set; }
    public int OrganizationCount { get; set; }

    // ----- Chart data -----
   // ----- Chart data -----
    public ChartSeries MonthlyTrend { get; set; } = new();
    public ChartSeries YearlyTrend { get; set; } = new();

    // ----- Customer Analytics section -----
    public System.Collections.Generic.List<RecentCustomer> RecentCustomers { get; set; } = new();

    public int CurrentYearNewCustomers { get; set; }
    public int PreviousYearNewCustomers { get; set; }

    // Null when there's no prior-year data to compare against.
    public double? YearOverYearGrowthPercent { get; set; }
}

// A trimmed-down record for the "Recently added customers" list --
// only what that widget needs to display, not the full Customer model.
public class RecentCustomer
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? DisplayName { get; set; }
    public System.DateTime? CreatedOn { get; set; }
}

// Simple label/value pairing that maps directly onto a Chart.js dataset
// once serialized to JSON in the view.
public class ChartSeries
{
    public System.Collections.Generic.List<string> Labels { get; set; } = new();
    public System.Collections.Generic.List<int> Values { get; set; } = new();
}