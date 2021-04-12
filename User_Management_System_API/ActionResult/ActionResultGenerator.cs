using User_Management_System_API.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace User_Management_System_API.ActionResult
{
    public static class ActionResultGenerator
    {
        public static IActionResult ErrorResult(HttpStatusCode code, string message, string source)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new ObjectResult(new CreateErrorResponse(code, message, source))
            {
                StatusCode = (int)code
            };
        }

        public static IActionResult SuccessResult(HttpStatusCode code, object result, string source)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException(nameof(source));
            }
            return new JsonResult(new { code, result, source });
        }
    }
}
