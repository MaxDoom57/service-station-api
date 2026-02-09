using Application.DTOs.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
    [Route("api/ssms/v0.1/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly CustomerService _service;

    public CustomerController(CustomerService service)
    {
        _service = service;
    }
    //----------------------------------------
    //Endpoints start here
    //----------------------------------------
    //Get all customers
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _service.GetCustomersAsync();
        return Ok(customers);
    }

    //Add a new customer
    [HttpPost]
    public async Task<IActionResult> AddCustomer([FromBody] AddCustomerAddressDto dto)
    {
        try
        {
            int adrKy = await _service.AddCustomerAsync(dto);
            return StatusCode(201, new
            {
                message = "Customer added successfully",
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerAddressDto dto)
    {
        try
        {
            await _service.UpdateCustomerAsync(dto);
            return Ok(new { message = "Customer updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }

    }
}
