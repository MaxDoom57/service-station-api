using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/ssms/v0.1/calendar")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly IDynamicDbContextFactory _factory;

        public CalendarController(IDynamicDbContextFactory factory)
        {
            _factory = factory;
        }

        [HttpGet("unavailable-dates")]
        public async Task<IActionResult> GetUnavailableDates()
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();
                var today = DateTime.Today;

                var dates = await db.CalendarMas
                    .Where(c => c.CalDt >= today && !c.fInAct)
                    .Select(c => new 
                    {
                        Date = c.CalDt,
                        Description = c.CalDesc
                    })
                    .ToListAsync();

                return Ok(dates);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
