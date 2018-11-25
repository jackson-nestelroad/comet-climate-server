using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace Comet.Climate.Server.Controllers
{
    [Route("/error")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        // Some sort of error occurred
        [AllowAnonymous]
        public IActionResult Error()
        {
            // Get the status code from the exception or the web server
            var statusCode = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error is HttpException httpEx ?
                httpEx.StatusCode : (HttpStatusCode) Response.StatusCode;

            // Get reason for the error based on the code
            string reason;
            switch(statusCode)
            {
                case HttpStatusCode.OK: reason = "Invalid request."; break;
                case HttpStatusCode.InternalServerError: reason = "Internal server error."; break;
                default: reason = "Unknown."; break;
            }

            // JSON object to return
            var result = new {
                success = false,
                reason = reason,
                code = statusCode
            };

            return BadRequest(result);
        }
    }
}
