using System;

namespace CustomerList.Models.ViewModels;

// Curated shape for the table view. Id is kept here for internal use
// (e.g. building a details/edit link) but is not shown as its own column.
public class CustomerViewModel
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? DisplayName { get; set; }
    public bool IsPerson { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }

    public static CustomerViewModel FromCustomer(Customer customer)
    {
        return new CustomerViewModel
        {
            Id = customer.Id,
            Code = customer.Code,
            DisplayName = customer.FullName,
            IsPerson = customer.IsPerson,
            IsActive = customer.IsActive,
            StartDate = customer.StartDate
        };
    }
}
