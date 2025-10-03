using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class DoctorDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
