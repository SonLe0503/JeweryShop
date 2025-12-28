using AutoMapper;
using Google.Apis.Auth;
using JewelryShop.DTO;
using JewelryShop.Models;
using JewelryShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JewelryShop.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly EmailVerificationService _emailService;
        public AuthController(JewelryShopContext context, IConfiguration configuration, IMapper mapper, IWebHostEnvironment env, EmailVerificationService emailService)
        {
            _context = context;
            _config = configuration;
            _mapper = mapper;
            _env = env;
            _emailService = emailService;
        }

        [HttpPost("Google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest body)
        {
            try
            {
                if (string.IsNullOrEmpty(body.IdToken))
                    return BadRequest(new { message = "Thiếu id_token" });

                var payload = await GoogleJsonWebSignature.ValidateAsync(body.IdToken);
                var user = _context.Users.FirstOrDefault(u => u.Email == payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = payload.Email,
                        Role = "Customer",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        Avatar = payload.Picture,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()) 
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    Issuer = _config["Jwt:Issuer"],
                    Audience = _config["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                string jwt = tokenHandler.WriteToken(token);

                return Ok(new
                {
                    message = "Đăng nhập Google thành công",
                    token = jwt
                });
            }
            catch (InvalidJwtException)
            {
                return Unauthorized(new { message = "Token Google không hợp lệ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Register(RegisterRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
                return BadRequest(new { message = "Email đã được đăng ký." });

            var existingVerification = _context.EmailVerifications
                .FirstOrDefault(v => v.Email == request.Email);
            if (existingVerification != null)
                _context.EmailVerifications.Remove(existingVerification);

            var token = _emailService.GenerateVerificationToken();
            var verification = new EmailVerification
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpirationTime = DateTime.UtcNow.AddMinutes(30)
            };

            _context.EmailVerifications.Add(verification);
            _context.SaveChanges();

            _emailService.SendVerificationEmail(request.Email, token);

            return Ok(new
            {
                message = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản."
            });
        }

        [HttpGet]
        public IActionResult VerifyEmail(string token)
        {
            var verification = _context.EmailVerifications
                .FirstOrDefault(v => v.Token == token);

            if (verification == null || verification.ExpirationTime < DateTime.UtcNow)
                return BadRequest(new { message = "Liên kết không hợp lệ hoặc đã hết hạn." });

            if (_context.Users.Any(u => u.Email == verification.Email))
            {
                _context.EmailVerifications.Remove(verification);
                _context.SaveChanges();
                return Ok(new { message = "Email này đã được xác thực trước đó." });
            }

            var user = new User
            {
                Email = verification.Email,
                PasswordHash = verification.PasswordHash,
                PhoneNumber = verification.PhoneNumber,
                Role = "Customer",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.EmailVerifications.Remove(verification);
            _context.SaveChanges();

            return Ok(new { message = "Xác thực email thành công! Tài khoản đã được tạo." });
        }

        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Email không tồn tại " });
            }
            bool verified = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!verified)
            {
                return Unauthorized(new { message = "Mật khẩu không đúng " });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwt = tokenHandler.WriteToken(token);
            return Ok(new { message = "Đăng nhập thành công", token = jwt });
        }

        [HttpPost]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            if (string.IsNullOrEmpty(model.Email))
                return BadRequest(new { message = "Vui lòng nhập email." });

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
                return BadRequest(new { message = "Email không tồn tại trong hệ thống." });

            string tempPassword = Guid.NewGuid().ToString().Substring(0, 8); // 8 ký tự ngẫu nhiên

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            _context.Users.Update(user);
            _context.SaveChanges();

            SendTemporaryPasswordEmail(user.Email, tempPassword);

            return Ok(new { message = "Mật khẩu tạm đã được gửi đến email của bạn." });
        }

        private void SendTemporaryPasswordEmail(string email, string tempPassword)
        {
            string subject = "Mật khẩu tạm thời - Jewelry Shop";
            string body = $@"
        <h2>Xin chào!</h2>
        <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản tại Jewelry Shop.</p>
        <p>Mật khẩu tạm thời của bạn là: <b>{tempPassword}</b></p>
        <p>Vui lòng đăng nhập và đổi mật khẩu ngay để bảo mật tài khoản.</p>
        <p>Trân trọng,<br>Đội ngũ Jewelry Shop</p>";

            _emailService.SendEmail(email, subject, body);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.ToList();
            var userDtos = _mapper.Map<List<UserResponse>>(users);
            return Ok(userDtos);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            var userDto = _mapper.Map<UserResponse>(user);
            return Ok(userDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }
            user.Status = "Deleted";
            _context.SaveChanges();
            return Ok(new { message = $"Xóa tài khoản người dùng thành công ID = {id}" });
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (currentUserId != id && currentUserRole != "Admin")
            {
                return Forbid();
            }
            if (!string.IsNullOrEmpty(request.Email))
                user.Email = request.Email;

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrEmpty(request.Password))
            {
                if (currentUserRole != "Admin")
                {
                    if (string.IsNullOrEmpty(request.OldPassword))
                    {
                        return BadRequest(new { message = "Vui lòng nhập mật khẩu cũ để đổi mật khẩu" });
                    }

                    bool isOldPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);
                    if (!isOldPasswordCorrect)
                    {
                        return Unauthorized(new { message = "Mật khẩu cũ không đúng" });
                    }
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }


            if (currentUserRole == "Admin" && !string.IsNullOrEmpty(request.Role))
                user.Role = request.Role;

            _context.SaveChanges();

            var userDto = _mapper.Map<UpdateUserRequest>(user);
            return Ok(new { message = "Cập nhật thông tin thành công", user = userDto });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAvatar(int id, IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
                return BadRequest("Vui lòng chọn ảnh để tải lên.");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            string uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";
            string filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            user.Avatar = $"/uploads/avatars/{fileName}";
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật avatar thành công!",
                avatarUrl = user.Avatar
            });
        }
    }
}
