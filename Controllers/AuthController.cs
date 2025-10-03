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
using System.Security.Claims;
using System.Text;

namespace HospitalManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly DapperContext _context;
        private readonly IConfiguration _config;

        public AuthController(DapperContext dapper, IConfiguration config)
        {
            _context = dapper;
            _config = config;
        }

        [HttpGet]
        public IActionResult RegisterPatient() => View();

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

                // Optionally generate refresh token immediately
                await GenerateAndSaveRefreshToken(userId);

                return RedirectToAction("Login");
            }
            catch (Exception ex)
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
            if (!ModelState.IsValid)
                return View(dto);

            using var db = _context.CreateConnection();
            var user = await db.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email",
                new { dto.Email }
            );

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

            var token = await CreateToken(user);

            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(30),
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true
            });

            // Generate refresh token
            await GenerateAndSaveRefreshToken(user.UserId);

            return RedirectToAction("Dashboard", "Home");
        }

        private async Task<string> CreateToken(User user)
        {
            using var db = _context.CreateConnection();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            if (user.Role == "Patient")
            {
                var patientId = await db.ExecuteScalarAsync<int?>(
                    "SELECT PatientId FROM Patients WHERE UserId = @UserId",
                    new { user.UserId }
                );

                if (patientId.HasValue)
                    claims.Add(new Claim("PatientId", patientId.Value.ToString()));
            }

            if (user.Role == "Doctor")
            {
                var doctorId = await db.ExecuteScalarAsync<int?>(
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

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshToken(int userId)
        {
            var refreshToken = GenerateRefreshToken();
            var expiryDate = DateTime.UtcNow.AddDays(7);

            using var db = _context.CreateConnection();
            await db.ExecuteAsync(
                "INSERT INTO RefreshTokens (UserId, Token, ExpiryDate, Revoked) " +
                "VALUES (@UserId, @Token, @ExpiryDate, @Revoked)",
                new { UserId = userId, Token = refreshToken, ExpiryDate = expiryDate, Revoked = false }
            );

            return refreshToken;
        }

        private async Task<RefreshToken?> ValidateRefreshToken(string token, int userId)
        {
            using var db = _context.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<RefreshToken>(
                "SELECT * FROM RefreshTokens WHERE Token=@Token AND UserId=@UserId AND ExpiryDate > GETUTCDATE() AND Revoked=0",
                new { Token = token, UserId = userId }
            );
        }

        private async Task RevokeRefreshToken(string token)
        {
            using var db = _context.CreateConnection();
            await db.ExecuteAsync(
                "UPDATE RefreshTokens SET Revoked=1 WHERE Token=@Token",
                new { Token = token }
            );
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto dto)
        {
            var refreshToken = await ValidateRefreshToken(dto.RefreshToken, dto.UserId);
            if (refreshToken == null)
                return Unauthorized("Invalid or expired refresh token");

            await RevokeRefreshToken(dto.RefreshToken);

            using var db = _context.CreateConnection();
            var user = await db.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE UserId = @UserId",
                new { UserId = dto.UserId }
            );

            if (user == null)
                return Unauthorized();

            var newToken = await CreateToken(user);
            var newRefreshToken = await GenerateAndSaveRefreshToken(user.UserId);

            return Ok(new TokenResponseDto
            {
                AccessToken = newToken,
                RefreshToken = newRefreshToken
            });
        }
    }
}
