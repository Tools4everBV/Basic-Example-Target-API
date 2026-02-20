using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EXAMPLE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
            return Ok(new
            {
                AppVersion = version,
                BuildDate = System.IO.File.GetLastWriteTime(typeof(Program).Assembly.Location)
            });
        }
    }
}
