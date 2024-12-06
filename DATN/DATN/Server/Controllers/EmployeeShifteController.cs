
using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeShifteController : ControllerBase
    {
        private readonly EmployeeShifteService _employeeShifteService;

        public EmployeeShifteController(EmployeeShifteService employeeShifteService)
        {
            _employeeShifteService = employeeShifteService;
        }

        [HttpGet("GetEmployeeShifte")]
        public IActionResult GetEmployeeShifte()
        {
            try
            {
                var result = _employeeShifteService.GetEmployeeShifte();
                if (result == null || result.Count == 0)
                {
                    return NotFound("Không có EmployeeShifte nào.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPost("AddEmployeeShifte")]
        public IActionResult AddEmployeeShifte([FromBody] EmployeeShifte employeeShifte)
        {
            if (employeeShifte == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            try
            {
                var result = _employeeShifteService.AddEmployeeShifte(employeeShifte);
                return CreatedAtAction(nameof(GetIdEmployeeShifte), new { id = result.EmployeeShifteId }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetIdEmployeeShifte(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID không hợp lệ.");
            }

            try
            {
                var result = _employeeShifteService.GetIdEmployeeShifte(id);
                if (result == null)
                {
                    return NotFound($"Không tìm thấy EmployeeShifte với ID {id}.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEmployeeShifte(int id)
        {
            try
            {
                var deleted = _employeeShifteService.DeleteEmployeeShifte(id);
                if (deleted == null)
                {
                    return NotFound($"Không tìm thấy EmployeeShifte với ID {id}.");
                }

                return Ok(deleted);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployeeShifte(int id, [FromBody] EmployeeShifte updatedEmployeeShifte)
        {
            if (updatedEmployeeShifte == null || id <= 0)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            try
            {
                var updated = _employeeShifteService.UpdateEmployeeShifte(id, updatedEmployeeShifte);
                if (updated == null)
                {
                    return NotFound($"Không tìm thấy EmployeeShifte với ID {id}.");
                }

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }
    }
}
