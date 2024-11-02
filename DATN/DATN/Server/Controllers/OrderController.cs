using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _OrderService;

        public OrderController(OrderService _order)
        {
            _OrderService = _order;
        }

        [HttpGet("GetOrder")]
        public List<Order> GetOrder()
        {
            return _OrderService.GetOrder();
        }

        [HttpPost("GetOrderStatus")]
        public ActionResult<Order> GetOrderStatus([FromBody] int tableId)
        {
            if (tableId <= 0) {return BadRequest("Value must be...");}

            return Ok(_OrderService.GetOrderStatus(tableId));
        }

        [HttpGet("GetOC")]
        public ActionResult<List<Order>> GetOC()
        {
            return Ok(_OrderService.GetOC());
        }

        [HttpPost("AddOrder")]
        public Order AddOrder(Order Order)
        {
            return _OrderService.AddOrder(new Order
            {
                TableId = Order.TableId,
                CreateDate = Order.CreateDate,
                TotalAmount = Order.TotalAmount,
                Status = Order.Status,
                PaymentMethod = Order.PaymentMethod,
                CustomerId = Order.CustomerId,
                CustomerVoucherId = Order.CustomerVoucherId,
                EmployeeId = Order.EmployeeId,
                Note = Order.Note,
                IsDeleted = Order.IsDeleted
            });
        }

        [HttpGet("{id}")]
        public ActionResult<Order> GetIdOrder(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_OrderService.GetIdOrder(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var deleted = _OrderService.DeleteOrder(id);
            if (deleted == null)
            {
                return NotFound("Order not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateOrder(int id, [FromBody] Order updatedOrder)
        {
            var updated = _OrderService.UpdateOrder(id, updatedOrder);
            if (updated == null)
            {
                return NotFound("Order not found");
            }

            return Ok(updated);
        }
    }
}
