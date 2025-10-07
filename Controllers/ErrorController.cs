using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature != null)
            {
               
                var path = exceptionHandlerPathFeature.Path;
                var message = exceptionHandlerPathFeature.Error.Message;
                ViewBag.ErrorMessage = message;
                ViewBag.Path = path;
            }
            else
            {
                ViewBag.ErrorMessage = "An unexpected error occurred.";
                ViewBag.Path = "";
            }

            return View();
        }
    }
}
