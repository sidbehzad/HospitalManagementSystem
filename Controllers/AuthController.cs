
using HospitalManagementSystem.Dtos.Auth;
using HospitalManagementSystem.Repository.Auths;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
                TempData["Success"] = "Registration successful! You can now login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Check if exception is due to duplicate email or contact
                if (ex.Message.Contains("UNIQUE") || ex.Message.Contains("duplicate"))
                {
                    if (ex.Message.Contains("Email"))
                        ModelState.AddModelError("Email", "This email is already registered.");
                    if (ex.Message.Contains("Contact"))
                        ModelState.AddModelError("Contact", "This phone number is already registered.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                }

                return View(dto);
            }
        }


        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var tokens = await _repo.LoginAsync(dto);

                if (tokens == null)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect email or password."); 
                    return View(dto);
                }

                Response.Cookies.Append("AuthToken", tokens.AccessToken, new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Secure = true
                });

                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(tokens.AccessToken);
                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                return role switch
                {
                    "Admin" => RedirectToAction("Index", "AdminDashboard"),
                    "Doctor" => RedirectToAction("Index", "DoctorDashboard"),
                    "Patient" => RedirectToAction("Index", "PatientDashboard"),
                    _ => RedirectToAction("Index", "Home")
                };
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

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Remove JWT cookie
            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                Response.Cookies.Delete("AuthToken");
            }

            // Redirect to login page
            return RedirectToAction("Login", "Auth");
        }

    }
}
