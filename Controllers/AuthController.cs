
using HospitalManagementSystem.Dtos.Auth;
using HospitalManagementSystem.Repository.Auths;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthsRepository _repo;

        public AuthController(IAuthsRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult RegisterPatient() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterPatient(RegisterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _repo.RegisterPatientAsync(dto);
                return RedirectToAction("Login");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return View(dto);
            }
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var tokens = await _repo.LoginAsync(dto);
                if (tokens == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid credentials");
                    return View(dto);
                }

                Response.Cookies.Append("AuthToken", tokens.AccessToken, new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Secure = true
                });

                return RedirectToAction("Dashboard", "Home");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Login failed. Please try again.");
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto dto)
        {
            try
            {
                var tokens = await _repo.RefreshTokenAsync(dto);
                if (tokens == null) return Unauthorized("Invalid or expired refresh token");

                return Ok(tokens);
            }
            catch
            {
                return StatusCode(500, "An error occurred while refreshing the token.");
            }
        }
    }
}
