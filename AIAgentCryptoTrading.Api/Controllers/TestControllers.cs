using Microsoft.AspNetCore.Mvc;
// Include any other using statements that your other controllers have

namespace YourProject.Controllers // Use the same namespace as your other controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Backend is connected!" });
        }
    }
}