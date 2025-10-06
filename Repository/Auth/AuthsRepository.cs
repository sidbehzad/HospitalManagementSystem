using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos.Auth;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HospitalManagementSystem.Repository.Auths
{
    public class AuthsRepository: IAuthsRepository
    {
        private readonly DapperContext _context;
        private readonly IConfiguration _config;

        public AuthsRepository(DapperContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task RegisterPatientAsync(RegisterDto dto)
        {
            try
            {
                dto.Password = new PasswordHasher<User>().HashPassword(null, dto.Password);

                using var db = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@Email", dto.Email);
                parameters.Add("@PasswordHash", dto.Password);
                parameters.Add("@Name", dto.Name);
                parameters.Add("@Age", dto.Age);
                parameters.Add("@Gender", dto.Gender);
                parameters.Add("@Contact", dto.Contact);
                parameters.Add("@Address", dto.Address);
                parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await db.ExecuteAsync("sp_RegisterPatient", parameters, commandType: CommandType.StoredProcedure);
                var userId = parameters.Get<int>("@UserId");

                // Generate refresh token
                var refreshToken = GenerateRefreshToken();
                await db.ExecuteAsync(
                    "INSERT INTO RefreshTokens (UserId, Token, ExpiryDate, Revoked) VALUES (@UserId, @Token, @ExpiryDate, 0)",
                    new { UserId = userId, Token = refreshToken, ExpiryDate = DateTime.UtcNow.AddDays(7) }
                );
            }
            catch
            {
                throw new Exception("Registration failed. Please try again.");
            }
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
        {
            try
            {
                using var db = _context.CreateConnection();
                var user = await db.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Email=@Email",
                    new { dto.Email });

                if (user == null) return null;

                var result = new PasswordHasher<User>().VerifyHashedPassword(null, user.PasswordHash, dto.Password);
                if (result == PasswordVerificationResult.Failed) return null;

                var token = await CreateToken(user);
                var refreshToken = GenerateRefreshToken();

                await db.ExecuteAsync(
                    "INSERT INTO RefreshTokens (UserId, Token, ExpiryDate, Revoked) VALUES (@UserId, @Token, @ExpiryDate, 0)",
                    new { UserId = user.UserId, Token = refreshToken, ExpiryDate = DateTime.UtcNow.AddDays(7) }
                );

                return new TokenResponseDto
                {
                    AccessToken = token,
                    RefreshToken = refreshToken
                };
            }
            catch
            {
                throw new Exception("Login failed. Please try again.");
            }
        }

        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            try
            {
                using var db = _context.CreateConnection();
                var refreshToken = await db.QueryFirstOrDefaultAsync<RefreshToken>(
                    "SELECT * FROM RefreshTokens WHERE Token=@Token AND UserId=@UserId AND ExpiryDate > GETUTCDATE() AND Revoked=0",
                    new { Token = dto.RefreshToken, UserId = dto.UserId }
                );

                if (refreshToken == null) return null;

                await db.ExecuteAsync(
                    "UPDATE RefreshTokens SET Revoked=1 WHERE Token=@Token",
                    new { Token = dto.RefreshToken }
                );

                var user = await db.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE UserId=@UserId",
                    new { UserId = dto.UserId }
                );

                if (user == null) return null;

                var token = await CreateToken(user);
                var newRefreshToken = GenerateRefreshToken();

                await db.ExecuteAsync(
                    "INSERT INTO RefreshTokens (UserId, Token, ExpiryDate, Revoked) VALUES (@UserId, @Token, @ExpiryDate, 0)",
                    new { UserId = user.UserId, Token = newRefreshToken, ExpiryDate = DateTime.UtcNow.AddDays(7) }
                );

                return new TokenResponseDto
                {
                    AccessToken = token,
                    RefreshToken = newRefreshToken
                };
            }
            catch
            {
                throw new Exception("Refreshing token failed.");
            }
        }

        // Helper methods
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
                    "SELECT PatientId FROM Patients WHERE UserId=@UserId",
                    new { user.UserId });
                if (patientId.HasValue) claims.Add(new Claim("PatientId", patientId.Value.ToString()));
            }

            if (user.Role == "Doctor")
            {
                var doctorId = await db.ExecuteScalarAsync<int?>(
                    "SELECT DoctorId FROM Doctors WHERE UserId=@UserId",
                    new { user.UserId });
                if (doctorId.HasValue) claims.Add(new Claim("DoctorId", doctorId.Value.ToString()));
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
    }
}
