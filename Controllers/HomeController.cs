using Microsoft.AspNetCore.Mvc;

namespace WebAppAutorization.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [ApiController]
        [Route("api/[controller]")]
        public class MobileController : ControllerBase
        {
            [HttpGet]
            public IActionResult Get()
            {
                return Ok(new { message = "Hello from ASP.NET Core!" });
            }

            [HttpPost]
            public IActionResult Post([FromBody] dynamic data)
            {
                return Ok(new { received = data });
            }
        }
    }
}
