using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class PatientDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
