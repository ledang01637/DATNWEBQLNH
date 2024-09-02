using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _EmployeeService;

        public EmployeeController(EmployeeService _employee)
        {
            _EmployeeService = _employee;
        }

        [HttpGet("GetEmployee")]
        public List<Employee> GetEmployee()
        {
            return _EmployeeService.GetEmployee();
        }

        [HttpPost("AddEmployee")]
        public Employee AddEmployee(Employee Employee)
        {
            return _EmployeeService.AddEmployee(new Employee
            {
                EmployeeName = Employee.EmployeeName,
                Address = Employee.Address,
                PhoneNumber = Employee.PhoneNumber,
                Email = Employee.Email,
                Position = Employee.Position,
                HireDate = Employee.HireDate,
                Salary = Employee.Salary,
                IsDeleted = Employee.IsDeleted,
                AccountId = Employee.AccountId,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<Employee> GetIdEmployee(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_EmployeeService.GetIdEmployee(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteEmployee(int id)
        {
            var deleted = _EmployeeService.DeleteEmployee(id);
            if (deleted == null)
            {
                return NotFound("Unit not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(int id, [FromBody] Employee updatedEmployee)
        {
            var updated = _EmployeeService.UpdateEmployee(id, updatedEmployee);
            if (updated == null)
            {
                return NotFound("Employee not found");
            }

            return Ok(updated);
        }
    }
}
