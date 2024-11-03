﻿using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;
using System;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _CustomerService;

        public CustomerController(CustomerService _unit)
        {
            _CustomerService = _unit;
        }

        [HttpGet("GetCustomer")]
        public List<Customer> GetCustomer()
        {
            return _CustomerService.GetCustomer();
        }

        [HttpPost("GetCustomerByAccountId")]
        public ActionResult<Customer> GetCustomerByAccountId([FromBody] int accountId)
        {
            try
            {
                if (accountId <= 0)
                {
                    return BadRequest(new { message = "Account ID must be greater than zero." });
                }

                var customer = _CustomerService.GetCustomerByAccountId(accountId);
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found." });
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        [HttpGet("GetCustomerInclude")]
        public List<Customer> GetCustomerInclude()
        {
            return _CustomerService.GetCustomerInclude();
        }

        [HttpPost("AddCustomer")]
        public Customer AddCustomer(Customer Customer)
        {
            return _CustomerService.AddCustomer(new Customer
            {
                CustomerName = Customer.CustomerName,
                PhoneNumber = Customer.PhoneNumber,
                Address = Customer.Address,
                Email = Customer.Email,
                IsDeleted = Customer.IsDeleted,
                AccountId = Customer.AccountId,
                TotalRewardPoint = Customer.TotalRewardPoint,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<Customer> GetIdCustomer(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_CustomerService.GetIdCustomer(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            var deleted = _CustomerService.DeleteCustomer(id);
            if (deleted == null)
            {
                return NotFound("Customer not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCustomer(int id, [FromBody] Customer updatedCustomer)
        {
            var updated = _CustomerService.UpdateCustomer(id, updatedCustomer);
            if (updated == null)
            {
                return NotFound("Customer not found");
            }

            return Ok(updated);
        }
    }
}
