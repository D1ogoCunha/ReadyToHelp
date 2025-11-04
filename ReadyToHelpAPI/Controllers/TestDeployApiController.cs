using System;
using Microsoft.AspNetCore.Mvc;

namespace ReadyToHelpAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestDeployApiController : ControllerBase
    {
        // GET: api/TestDeployApi/ping
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                status = "ok",
                message = "Ping recebido - Swagger atualizado",
                utc = DateTime.UtcNow
            });
        }
    }
}