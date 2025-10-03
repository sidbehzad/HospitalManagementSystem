using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos.Auth;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Text;

namespace HospitalManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly DapperContext _context;
        private readonly IConfiguration _config;

        public AuthController(DapperContext dapper,IConfiguration config)
        {
            _config = config;
            _context = dapper;  
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> RegisterPatient()
        { 
            return  View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterPatient(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            using var db = _context.CreateConnection();
            

            try
            {
                var hashedPassword = new PasswordHasher<User>().HashPassword(null, dto.Password);

                var parameters = new DynamicParameters();
                parameters.Add("@Email", dto.Email);
                parameters.Add("@PasswordHash", hashedPassword);
                parameters.Add("@Name", dto.Name);
                parameters.Add("@Age", dto.Age);
                parameters.Add("@Gender", dto.Gender);
                parameters.Add("@Contact", dto.Contact);
                parameters.Add("@Address", dto.Address);
                parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await db.ExecuteAsync(
                    "sp_RegisterPatient",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var userId = parameters.Get<int>("@UserId");

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            using var db= _context.CreateConnection();

            var user = await db.QueryFirstOrDefaultAsync<User>("Select * from Users where Email=@Email", new { @Email = dto.Email });
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials");
                return View(dto);
            }

            var result = new PasswordHasher<User>().VerifyHashedPassword(null, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials");
                return View(dto);
            }
            var token = CreateToken(user);

            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(7),
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true
            });
            return RedirectToAction("Dashboard", "Home");
        }


        private string CreateToken(User user) 
        {
            using var db = _context.CreateConnection();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Role,user.Role),

                
            };

            if (user.Role == "Patient")
            {
                var patientId = db.ExecuteScalar<int?>(
                    "SELECT PatientId FROM Patients WHERE UserId = @UserId",
                    new { user.UserId }
                );

                if (patientId.HasValue)
                    claims.Add(new Claim("PatientId", patientId.Value.ToString()));
            }

            if (user.Role == "Doctor")
            {
                var doctorId = db.ExecuteScalar<int?>(
                    "SELECT DoctorId FROM Doctors WHERE UserId = @UserId",
                    new { user.UserId }
                );

                if (doctorId.HasValue)
                    claims.Add(new Claim("DoctorId", doctorId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        


    }
}
