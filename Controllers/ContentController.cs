using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RbacApi.Authorization;

namespace RbacApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult CreateContent()
        {
            // Your logic here
            return Ok(new { message = "Content created successfully" });
        }

        [HttpPut]
        [Authorize(Policy = "EditorPolicy")]
        public IActionResult EditContent()
        {
            // Your logic here
            return Ok(new { message = "Content edited successfully" });
        }

        [HttpDelete]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult DeleteContent()
        {
            // Your logic here
            return Ok(new { message = "Content deleted successfully" });
        }

        [HttpGet]
        [Authorize(Policy = "ViewerPolicy")]
        public IActionResult ViewContent()
        {
            // Your logic here
            return Ok(new { message = "Content viewed successfully" });
        }
    }
}