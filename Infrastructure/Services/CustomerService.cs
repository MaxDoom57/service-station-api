using Application.DTOs.Customers;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Constants;

public class CustomerService
{
    private readonly IAgentJobDispatcher _dispatcher;
    private readonly IUserRequestContext _userContext;
    private readonly IUserKeyService _userKeyService;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        IAgentJobDispatcher dispatcher,
        IUserRequestContext userContext,
        IUserKeyService userKeyService,
        ILogger<CustomerService> logger)
    {
        _dispatcher     = dispatcher;
        _userContext    = userContext;
        _userKeyService = userKeyService;
        _logger         = logger;
    }

    // Get all active customers
    public async Task<List<CustomerDto>> GetCustomersAsync()
    {
        _logger.LogInformation("[{Method}] Dispatching {Job}", nameof(GetCustomersAsync), AgentJobTypes.GetCustomers);

        var result = await _dispatcher.DispatchAndWaitAsync(
            companyKey: _userContext.CompanyKey,
            jobType:    AgentJobTypes.GetCustomers,
            payload:    new { CompanyKey = _userContext.CompanyKey });

        if (!result.Success)
        {
            _logger.LogError("[{Method}] Agent error: {Error}", nameof(GetCustomersAsync), result.Error);
            throw new Exception(result.Error ?? "Agent error");
        }

        var customers = result.Deserialize<List<CustomerDto>>() ?? new();
        _logger.LogInformation("[{Method}] Returned {Count} customers", nameof(GetCustomersAsync), customers.Count);

        return customers;
    }

    // Add new customer address (complex multi-table transaction — delegated fully to agent)
    public async Task<int> AddCustomerAsync(AddCustomerAddressDto dto)
    {
        // Pre-validate on the API side (no DB needed)
        if (!string.IsNullOrWhiteSpace(dto.EMail))
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.EMail, pattern))
                throw new Exception("Invalid email format");
        }

        if (!IsValidMobile(dto.TP1))  throw new Exception("TP1 must contain exactly 10 digits");
        if (!IsValidMobile(dto.TP2))  throw new Exception("TP2 must contain exactly 10 digits");

        var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
        if (userKey == null)
            throw new Exception("User key not found for this user");

        var result = await _dispatcher.DispatchAndWaitAsync(
            companyKey: _userContext.CompanyKey,
            jobType:    AgentJobTypes.AddCustomer,
            payload:    new
            {
                Dto        = dto,
                UserKey    = userKey.Value,
                OurCd      = dto.ourCd ?? "CUS",
                CompanyKey = _userContext.CompanyKey
            });

        if (!result.Success)
            throw new Exception(result.Error ?? "Failed to add customer");

        return result.Deserialize<int>();
    }

    public async Task UpdateCustomerAsync(UpdateCustomerAddressDto dto)
    {
        // Pre-validate on the API side
        if (!string.IsNullOrWhiteSpace(dto.EMail))
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.EMail, pattern))
                throw new Exception("Invalid email format");
        }

        if (!IsValidMobile(dto.TP1)) throw new Exception("TP1 must contain exactly 10 digits");
        if (!IsValidMobile(dto.TP2)) throw new Exception("TP2 must contain exactly 10 digits");

        var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
        if (userKey == null)
            throw new Exception("User key not found for this user");

        var result = await _dispatcher.DispatchAndWaitAsync(
            companyKey: _userContext.CompanyKey,
            jobType:    AgentJobTypes.UpdateCustomer,
            payload:    new { Dto = dto, UserKey = userKey.Value });

        if (!result.Success)
            throw new Exception(result.Error ?? "Failed to update customer");
    }

    private static bool IsValidMobile(string? number)
    {
        if (string.IsNullOrWhiteSpace(number)) return true;
        return number.Length == 10 && number.All(char.IsDigit);
    }
}
