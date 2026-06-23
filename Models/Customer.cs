using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace CustomerList.Models;

// Mirrors the Customer record exactly as returned by the
// /api/consignee/dynamic API endpoint.
//
// NOTE: the API is not fully type-consistent across records -- some
// fields documented/observed as strings (e.g. "title") can occasionally
// arrive as raw JSON numbers for certain records. Every string property
// below uses LenientStringConverter so a stray number/bool never crashes
// deserialization for the whole list.
public class Customer
{
    public int Id { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? Code { get; set; }

    [JsonConverter(typeof(LenientNullableIntConverter))]
    public int? GslType { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? Tin { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? BioId { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? NationalId { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? PassportId { get; set; }

    public bool IsPerson { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? Title { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? FirstName { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? SecondName { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? ThirdName { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? Gender { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? BusinessType { get; set; }

    [JsonConverter(typeof(LenientNullableIntConverter))]
    public int? Preference { get; set; }
    public DateTime? StartDate { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? Nationality { get; set; }

    public bool IsActive { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? MaritalStatus { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? Note { get; set; }

    public DateTime? CreatedOn { get; set; }
    public DateTime? LastModified { get; set; }
    [JsonConverter(typeof(LenientNullableIntConverter))]
    public int? MainConsigneeUnit { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? BaseUrl { get; set; }

    [JsonConverter(typeof(LenientNullableIntConverter))]
    public int? ParentId { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? Department { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? Branch { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? Position { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? CommunicationSource { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? DefaultLanguage { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? DefaultCurrency { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? DefaultImageUrl { get; set; }

    public decimal? CreditLimit { get; set; }
    public decimal? TransactionLimit { get; set; }
    public bool Locked { get; set; }

    [JsonConverter(typeof(LenientStringConverter))]
    public string? Remark { get; set; }

    // Combines FirstName/SecondName/ThirdName into one display string,
    // skipping any that are null or empty.
    public string FullName =>
        string.Join(" ", new[] { FirstName, SecondName, ThirdName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
}
