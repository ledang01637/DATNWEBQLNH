using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RewardPointeController : ControllerBase
    {
        private readonly RewardPointeService _RewardPointeService;

        public RewardPointeController(RewardPointeService _rewardPointe)
        {
            _RewardPointeService = _rewardPointe;
        }

        [HttpGet("GetRewardPointe")]
        public List<RewardPointe> GetRewardPointe()
        {
            return _RewardPointeService.GetRewardPointe();
        }

        [HttpPost("AddRewardPointe")]
        public RewardPointe AddRewardPointe(RewardPointe RewardPointe)
        {
            return _RewardPointeService.AddRewardPointe(new RewardPointe
            {
                CustomerId = RewardPointe.CustomerId,
                RewardPoint = RewardPointe.RewardPoint,
                UpdateDate = RewardPointe.UpdateDate,
                IsDeleted = RewardPointe.IsDeleted,
                OrderId = RewardPointe.OrderId,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<RewardPointe> GetIdRewardPointe(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_RewardPointeService.GetIdRewardPointe(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteRewardPointe(int id)
        {
            var deleted = _RewardPointeService.DeleteRewardPointe(id);
            if (deleted == null)
            {
                return NotFound("RewardPointe not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateRewardPointe(int id, [FromBody] RewardPointe updatedRewardPointe)
        {
            var updated = _RewardPointeService.UpdateRewardPointe(id, updatedRewardPointe);
            if (updated == null)
            {
                return NotFound("RewardPointe not found");
            }

            return Ok(updated);
        }
    }
}
