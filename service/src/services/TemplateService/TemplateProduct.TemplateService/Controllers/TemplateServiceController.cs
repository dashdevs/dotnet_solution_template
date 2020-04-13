using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TemplateCompany.TemplateProduct.TemplateService.Controllers
{
    [ApiController]
    public class TemplateServiceController : ControllerBase
    {
        [HttpGet]
        [Route("[controller]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetTemplateService()
        {
            return Ok("Hello from TemplateService!");
        }
    }
}
