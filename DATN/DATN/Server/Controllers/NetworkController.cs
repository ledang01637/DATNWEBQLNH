using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NetworkController : ControllerBase
    {
        private readonly NetworkService _networkService;
        public NetworkController(NetworkService networkService) { _networkService = networkService; }


        [HttpGet("wifi-ip")]
        public IActionResult GetWifiIpAddress()
        {
            var ipAddress = _networkService.GetWifiIPAddress();
            if (ipAddress != null)
            {
                return Ok(ipAddress);
            }
            return Ok("Wi-Fi IP Address not found.");
        }
    }
}
