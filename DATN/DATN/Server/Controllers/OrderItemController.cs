﻿using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly OrderItemService _OrderItemService;

        public OrderItemController(OrderItemService _orderItem)
        {
            _OrderItemService = _orderItem;
        }

        [HttpGet("GetOrderItem")]
        public List<OrderItem> GetOrderItem()
        {
            return _OrderItemService.GetOrderItem();
        }

        [HttpPost("GetOrderItemInclude")]
        public List<OrderItem> GetOrderItemInclude([FromBody] int orderId)
        {
            if(orderId < 0) { return null; }

            return _OrderItemService.GetOrderItemInclude(orderId);
        }

        [HttpPost("AddOrderItem")]
        public OrderItem AddOrderItem(OrderItem OrderItem)
        {
            return _OrderItemService.AddOrderItem(new OrderItem
            {
                OrderId = OrderItem.OrderId,
                ProductId = OrderItem.ProductId,
                Quantity = OrderItem.Quantity,
                Price = OrderItem.Price,
                TotalPrice = OrderItem.TotalPrice,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<OrderItem> GetIdOrderItem(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_OrderItemService.GetIdOrderItem(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteOrderItem(int id)
        {
            var deleted = _OrderItemService.DeleteOrderItem(id);
            if (deleted == null)
            {
                return NotFound("OrderItem not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateOrderItem(int id, [FromBody] OrderItem updatedOrderItem)
        {
            var updated = _OrderItemService.UpdateOrderItem(id, updatedOrderItem);
            if (updated == null)
            {
                return NotFound("OrderItem not found");
            }

            return Ok(updated);
        }
    }
}
