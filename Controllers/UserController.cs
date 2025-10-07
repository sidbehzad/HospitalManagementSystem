using HospitalManagementSystem.Dtos.User;

using HospitalManagementSystem.Models;
using HospitalManagementSystem.Repository.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly IUsersRepository _repo;

        public UserController(IUsersRepository repo)
        {
            _repo = repo;
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _repo.GetAdminsAndDoctorsAsync();
                return View(users);
            }
            catch
            {
                return StatusCode(500, "Unable to fetch users.");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterDoctor() => View();

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterDoctor(RegisterDoctorDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _repo.RegisterDoctorAsync(dto);
                return RedirectToAction("RegisterDoctor");
            }
            catch
            {
                ModelState.AddModelError("", "Registration failed. Please try again.");
                return View(dto);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(int id)
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(EditUserDto dto)
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                if (!await _repo.DeleteUserAsync(userId)) return NotFound();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest("Delete failed: " + ex.Message);
            }
        }
    }
}
