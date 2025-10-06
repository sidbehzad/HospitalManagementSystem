using HospitalManagementSystem.Dtos.User;
using HospitalManagementSystem.Repository.Users;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class DoctorDashboardController : Controller
    {
        private readonly IUsersRepository _repo;

        public DoctorDashboardController(IUsersRepository repo)
        {
            _repo = repo;
        }
        public IActionResult Index()
        {
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> EditPatient(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return NotFound();

            var dto = new EditUserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditPatient(EditUserDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                if (!await _repo.UpdateUserAsync(dto)) return NotFound();
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Error updating user.");
                return View(dto);
            }
        }
    }
}
