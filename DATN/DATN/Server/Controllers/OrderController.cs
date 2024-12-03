using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;
using System;

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
        public ActionResult<List<Order>> GetOrder()
        {
                var orders = _OrderService.GetOrder();
                return Ok(orders);
            
        }

        [HttpGet("GetOrderStatus")]
        public ActionResult<Order> GetOrderStatus([FromQuery] int tableId)
        {
            if (tableId <= 0) {return BadRequest("Value must be...");}

            return Ok(_OrderService.GetOrderStatus(tableId));
        }

        [HttpGet("GetOrderStatusTrans")]
        public ActionResult<Order> GetOrderStatusTrans([FromQuery] int tableId)
        {
            if (tableId <= 0) { return BadRequest("Value must be..."); }

            return Ok(_OrderService.GetOrderStatusTrans(tableId));
        }

        [HttpPost("GetOrderInvoice")]
        public ActionResult<Order> GetOrderInvoice([FromBody] int orderId)
        {
            if (orderId <= 0) { return BadRequest("Value must be..."); }

            return Ok(_OrderService.GetOrderInvoice(orderId));
        }

        [HttpGet("GetOrderLstInclude")]
        public ActionResult<List<Order>> GetOrderLstInclude()
        {
            return Ok(_OrderService.GetOrderLstInclude());
        }

        [HttpGet("GetOrderLstByCustomer")]
        public ActionResult<List<Order>> GetOrderLstByCustomer([FromQuery] int customerId)
        {
            if (customerId <= 0) { return BadRequest("Value must be..."); }

            return Ok(_OrderService.GetOrderLstByCustomer(customerId));
        }


        [HttpPost("AddOrder")]
        public Order AddOrder(Order Order)
        {
            return _OrderService.AddOrder(new Order
            {
                OrderId = Order.OrderId,
                TableId = Order.TableId,
                EmployeeId = Order.EmployeeId,
                CreateDate = Order.CreateDate,
                TotalAmount = Order.TotalAmount,
                Status = Order.Status,
                CustomerId = Order.CustomerId,
                PaymentMethod = Order.PaymentMethod,
                Note = Order.Note,
                CustomerVoucherId = Order.CustomerVoucherId,
                IsDeleted = Order.IsDeleted
            });
        }

        [HttpGet("{id}")]
        public ActionResult<Order> GetIdOrder(int id)
        {
            if (id <= 0)
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
