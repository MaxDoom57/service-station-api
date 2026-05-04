using Application.DTOs.Checklist;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/checklist")]
    [AllowAnonymous]
    public class ChecklistController : ControllerBase
    {
        private readonly ChecklistService _service;

        public ChecklistController(ChecklistService service)
        {
            _service = service;
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetChecklistItems()
        {
            try
            {
                var result = await _service.GetChecklistItemsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateChecklist([FromBody] SaveChecklistDto dto)
        {
            try
            {
                var result = await _service.CreateChecklistAsync(dto);
                return result.success ? Ok(result.message) : BadRequest(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{ordKy}")]
        public async Task<IActionResult> UpdateChecklist(int ordKy, [FromBody] List<ChecklistValueDto> items)
        {
            try
            {
                var result = await _service.UpdateChecklistAsync(ordKy, items);
                return result.success ? Ok(result.message) : BadRequest(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("master-item")]
        public async Task<IActionResult> CreateChecklistMasterItem ([FromBody] CreateChecklistMasterDto dto)
        {
            try
            {
                var result = await _service.CreateChecklistMasterAsync(dto);
                return result.success ? Ok(result.message) : BadRequest(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
