using HospitalManagementSystem.Dtos.Auth;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using HospitalManagementSystem.Data;
using System.Data;
using HospitalManagementSystem.Dtos;

namespace HospitalManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly DapperContext _context;

        public AdminController(DapperContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

       
        [HttpPost]
        public async Task<IActionResult> Register(AdminRegisterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var hasher = new PasswordHasher<User>();
                string hashedPassword = hasher.HashPassword(null, dto.Password);

                using var db = _context.CreateConnection();

                var sql = "INSERT INTO Users (Email, PasswordHash, Role) VALUES (@Email, @PasswordHash, 'Admin')";
                await db.ExecuteAsync(sql, new { dto.Email, PasswordHash = hashedPassword });

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
