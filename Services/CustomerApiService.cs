using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CustomerList.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerList.Services;

public class CustomerApiService
{
    private readonly HttpClient _httpClient;
    private readonly int _defaultGslType;
    private readonly ILogger<CustomerApiService> _logger;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "Customers_All";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CustomerApiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CustomerApiService> logger,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _defaultGslType = configuration.GetValue<int>("CustomerApi:DefaultGslType");
        _logger = logger;
        _cache = cache;

        var baseUrl = configuration["CustomerApi:BaseUrl"];
        if (!string.IsNullOrEmpty(baseUrl))
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    public async Task<List<Customer>> GetCustomersAsync(int? gslType = null)
    {
        // Serve from cache when available, so repeated page loads
        // (search/sort/page clicks) don't all re-hit the external API.
        if (_cache.TryGetValue(CacheKey, out List<Customer>? cached) && cached != null)
        {
            _logger.LogInformation("Returning cached customer data");
            return cached;
        }

        var type = gslType ?? _defaultGslType;
        var requestUrl = $"api/consignee/dynamic?gsltype={type}";

        try
        {
            var customers = await _httpClient.GetFromJsonAsync<List<Customer>>(requestUrl, JsonOptions);
            var result = customers ?? new List<Customer>();

            _cache.Set(CacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            });

            _logger.LogInformation("Fetched and cached {Count} customers from API", result.Count);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error fetching customers");
            throw new ApplicationException("Unable to connect to the Customer API. Please check the network connection.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Unexpected JSON shape from customer API");
            throw new ApplicationException("The Customer API returned unexpected data.", ex);
        }
    }
}
