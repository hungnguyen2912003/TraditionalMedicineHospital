using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Project.Validators;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly EmailService _emailService;
        private readonly AuthValidator _validator;
        private readonly JwtManager _jwtManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController
        (
            IUserRepository userRepository,
            AuthValidator validator,
            JwtManager jwtManager,
            EmailService emailService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration
        )
        {
            _userRepository = userRepository;
            _validator = validator;
            _jwtManager = jwtManager;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                var validationResult = _validator.ValidateLogin(username, password);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    var token = _jwtManager.GenerateToken(username, user.Role);
                    var expirationTime = DateTime.UtcNow.AddHours(1);
                    var cookieExpirationTime = expirationTime.AddMinutes(1);

                    Response.Cookies.Append("AuthToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = cookieExpirationTime
                    });

                    Response.Cookies.Append("TokenExpiration", ((DateTimeOffset)expirationTime).ToUnixTimeMilliseconds().ToString(), new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = cookieExpirationTime
                    });

                    if (user.IsFirstLogin)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Đây là lần đăng nhập đầu tiên của bạn. Vui lòng đổi mật khẩu!",
                            redirectUrl = Url.Action("ChangePassword", "Account", new { area = "Admin" }),
                            loginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        });
                    }

                    string redirectURL = user.Role.ToString() switch
                    {
                        "Admin" => Url.Action("Index", "Home", new { area = "Admin" }) ?? "/Admin/Home/Index",
                        "Nhanvien" => Url.Action("Index", "Home", new { area = "Staff" }) ?? "/Staff/Home/Index",
                        _ => Url.Action("") ?? "",
                    };

                    return Json(new
                    {
                        success = true,
                        message = "Đăng nhập thành công!",
                        redirectUrl = redirectURL,
                        loginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    });
                }
                return Json(new
                {
                    success = false,
                    message = "Mã nhân viên hoặc mật khẩu không đúng."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi đăng nhập: " + ex.Message
                });
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            var (username, role) = _jwtManager.GetClaimsFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                Response.Cookies.Delete("AuthToken");
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            ViewData["Username"] = username;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromForm] string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account", new { area = "Admin" });
                }

                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                {
                    Response.Cookies.Delete("AuthToken");
                    return RedirectToAction("Login", "Account", new { area = "Admin" });
                }

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "Admin" });
                }

                var validationResult = _validator.ValidateChangePassword(oldPassword, newPassword, confirmPassword, user.PasswordHash);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.IsFirstLogin = false;
                await _userRepository.UpdateAsync(user);

                //if (!string.IsNullOrEmpty(user.Employee.EmailAddress))
                //{
                //    var subject = "Thông báo thay đổi mật khẩu";
                //    var body = $"<h2>Xin chào {user.Employee.Name},</h2>" +
                //               "<p>Mật khẩu của bạn đã được thay đổi thành công vào lúc " +
                //               $"{DateTime.Now:dd/MM/yyyy HH:mm:ss}.</p>" +
                //               "<p>Nếu bạn không thực hiện hành động này, vui lòng liên hệ quản trị viên ngay lập tức.</p>" +
                //               "<p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                //    await _emailService.SendEmailAsync(user.Employee.EmailAddress, subject, body);
                //}

                Response.Cookies.Delete("AuthToken");

                return Json(new
                {
                    success = true,
                    message = "Thay đổi mật khẩu thành công! Vui lòng đăng nhập lại.",
                    redirectUrl = Url.Action("Login", "Account", new { area = "Admin" })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi mật khẩu: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPassword(string code)
        {
            try
            {
                var validationResult = _validator.ValidateForgotPassword(code);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                var employee = await _userRepository.GetByCodeAsync(code);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên với mã này." });
                }

                TempData["Code"] = code;
                return Json(new
                {
                    success = true,
                    message = "Xác nhận thành công! Vui lòng đặt lại mật khẩu.",
                    redirectUrl = Url.Action("ResetPassword", "Account", new { area = "Admin" })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserInfo()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { success = false, message = "Chưa đăng nhập." });
            }

            var (username, role) = _jwtManager.GetClaimsFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                Response.Cookies.Delete("AuthToken");
                return Unauthorized(new { success = false, message = "Token không hợp lệ." });
            }

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
            }

            if (role == "Admin")
            {
                return Json(new
                {
                    success = true,
                    username = username,
                    role = role
                });
            }
            else
            {
                var employee = user.Employee;
                if (employee == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin nhân viên." });
                }

                string imagePath = string.IsNullOrEmpty(employee.Images)
                    ? ""
                    : $"/Images/Employees/{employee.Images}";

                return Json(new
                {
                    success = true,
                    code = employee.Code,
                    id = employee.Id,
                    username = username,
                    name = employee.Name,
                    email = employee.EmailAddress,
                    image = imagePath,
                    role = role
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RenewToken()
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Không tìm thấy token." });
                }

                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                {
                    Response.Cookies.Delete("AuthToken");
                    Response.Cookies.Delete("TokenExpiration");
                    return Json(new { success = false, message = "Token không hợp lệ." });
                }

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                // Tạo token mới
                var newToken = _jwtManager.GenerateToken(username, user.Role);
                var expirationTime = DateTime.UtcNow.AddHours(1);
                var cookieExpirationTime = expirationTime.AddMinutes(1);

                // Cập nhật cookie với token mới
                Response.Cookies.Append("AuthToken", newToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = cookieExpirationTime
                });

                // Cập nhật cookie thời gian hết hạn
                Response.Cookies.Append("TokenExpiration", ((DateTimeOffset)expirationTime).ToUnixTimeMilliseconds().ToString(), new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = cookieExpirationTime
                });

                return Json(new { success = true, message = "Gia hạn token thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi gia hạn token: " + ex.Message });
            }
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
