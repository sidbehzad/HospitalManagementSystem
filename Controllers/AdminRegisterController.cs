using HospitalManagementSystem.Dtos.Auth;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using HospitalManagementSystem.Data;
using System.Data;

namespace HospitalManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly DapperContext _context;

        public AdminController(DapperContext context)
        {
            _context = context;
        }

        // GET: Show form to register admin
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Save new admin
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var hasher = new PasswordHasher<User>();
                string hashedPassword = hasher.HashPassword(null, dto.Password);

                using var db = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@Email", dto.Email);
                parameters.Add("@PasswordHash", hashedPassword);
                parameters.Add("@Name", dto.Name);
                parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await db.ExecuteAsync(
                    "INSERT INTO Users (Email, PasswordHash, Role) VALUES (@Email, @PasswordHash, 'Admin')",
                    new { dto.Email, PasswordHash = hashedPassword }
                );

                TempData["Success"] = "Admin registered successfully!";
                return RedirectToAction("Register");
            }
            catch
            {
                ModelState.AddModelError("", "Error registering admin. Try again.");
                return View(dto);
            }
        }
    }
}
