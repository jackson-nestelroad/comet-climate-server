using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace Comet.Climate.Server.Controllers
{
    // Wildcard URL that is not covered by any other controller
    [Route("{*url}", Order = 999)]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        // GET /...
        [HttpGet]
        public IActionResult Get()
        {
            var result = new {
                success = false,
                reason = "Invalid request.",
                code = System.Net.HttpStatusCode.NotFound
            };
            return NotFound(result);
        }
    }
}
