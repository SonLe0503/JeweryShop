using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public AuthController(JewelryShopContext context, IConfiguration configuration, IMapper mapper)
        {
            _context = context;
            _config = configuration;
            _mapper = mapper;
        }

        [HttpPost]
        public IActionResult Register(RegisterRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest("Email already exists");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = _mapper.Map<User>(request);
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(new { message = "User registered successfully" });
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
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwt = tokenHandler.WriteToken(token);
            return Ok(new { message = "Đăng nhập thành công", token = jwt });
        }

        [Authorize(Roles = "Admin")]
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
            _context.Users.Remove(user);
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

            // Nếu có password mới → mã hóa lại
            if (!string.IsNullOrEmpty(request.Password))
            {
                // Nếu là user thường → phải nhập mật khẩu cũ đúng
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

                // Hash mật khẩu mới
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            // Chỉ Admin mới được phép đổi role
            if (currentUserRole == "Admin" && !string.IsNullOrEmpty(request.Role))
                user.Role = request.Role;

            _context.SaveChanges();

            var userDto = _mapper.Map<UpdateUserRequest>(user);
            return Ok(new { message = "Cập nhật thông tin thành công", user = userDto });
        }
    }
}
