using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Project.Validators;
using Project.Helpers;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly EmailService _emailService;
        private readonly AuthValidator _validator;
        private readonly JwtManager _jwtManager;
        private readonly PasswordResetCodeHelper _resetCodeHelper;
        private const string CAPTCHA_KEY = "CaptchaCode";

        public AccountController
        (
            IUserRepository userRepository,
            AuthValidator validator,
            JwtManager jwtManager,
            EmailService emailService,
            PasswordResetCodeHelper resetCodeHelper
        )
        {
            _userRepository = userRepository;
            _validator = validator;
            _jwtManager = jwtManager;
            _emailService = emailService;
            _resetCodeHelper = resetCodeHelper;
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

                // Try to find user by username (employee code) or email
                var user = await _userRepository.GetByIdentifierAsync(username);
                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    var token = _jwtManager.GenerateToken(user.Username, user.Role);
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
                            redirectUrl = "/doi-mat-khau",
                            loginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        });
                    }

                    string redirectURL = user.Role.ToString() switch
                    {
                        "Admin" => "/admin",
                        "BacSi" => "/bac-si",
                        "YTa" => "/y-ta",
                        "NhanVienHanhChinh" => "/nhan-vien",
                        "BenhNhan" => "/benh-nhan",
                        _ => "/",
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
                    message = "Mã đăng nhập hoặc mật khẩu không đúng."
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

        [HttpGet("doi-mat-khau")]
        public IActionResult ChangePassword()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("/dang-nhap");
            }

            var (username, role) = _jwtManager.GetClaimsFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                Response.Cookies.Delete("AuthToken");
                return Redirect("/dang-nhap");
            }

            ViewData["Username"] = username;
            return View();
        }

        [HttpPost("doi-mat-khau")]
        public async Task<IActionResult> ChangePassword([FromForm] string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return Redirect("/dang-nhap");
                }

                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                {
                    Response.Cookies.Delete("AuthToken");
                    return Redirect("/dang-nhap");
                }

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    return Redirect("/dang-nhap");
                }

                var validationResult = _validator.ValidateChangePassword(oldPassword, newPassword, confirmPassword, user.PasswordHash);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.IsFirstLogin = false;
                await _userRepository.UpdateAsync(user);

                Response.Cookies.Delete("AuthToken");
                Response.Cookies.Delete("TokenExpiration");
                HttpContext.Session.Clear();

                return Json(new
                {
                    success = true,
                    message = "Thay đổi mật khẩu thành công! Vui lòng đăng nhập lại.",
                    redirectUrl = "/dang-nhap"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi mật khẩu: " + ex.Message });
            }
        }

        [HttpGet("dang-xuat")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("TokenExpiration");
            HttpContext.Session.Clear();
            return Redirect("/dang-nhap");
        }

        [HttpGet("quen-mat-khau")]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("quen-mat-khau")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromForm] string identifier, [FromForm] string captcha)
        {
            try
            {
                var storedCaptcha = HttpContext.Session.GetString(CAPTCHA_KEY);
                if (string.IsNullOrEmpty(storedCaptcha) || !storedCaptcha.Equals(captcha, StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Mã bảo vệ không đúng!" });
                }

                var user = await _userRepository.GetByIdentifierAsync(identifier);
                if (user == null || user.Employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng!" });
                }

                if (user.IsFirstLogin)
                {
                    return Json(new { success = false, message = "Tài khoản của bạn chưa đăng nhập lần đầu!" });
                }

                if (string.IsNullOrEmpty(user.Employee.EmailAddress))
                {
                    return Json(new { success = false, message = "Người dùng chưa cập nhật địa chỉ email!" });
                }

                var resetCode = _resetCodeHelper.GenerateResetCode(user.Username);
                var resetLink = Url.Action("ResetPassword", "Account",
                    new { area = "Admin", code = resetCode }, Request.Scheme);

                var subject = "Yêu cầu đặt lại mật khẩu";
                var body = $@"<h2>Xin chào {user.Employee.Name},</h2>
                            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                            <p>Vui lòng nhấn vào liên kết dưới đây để đặt lại mật khẩu:</p>
                            <p><a href='{resetLink}'>{resetLink}</a></p>
                            <p>Lưu ý: Liên kết này chỉ có hiệu lực trong vòng 5 giờ.</p>
                            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                            <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";

                await _emailService.SendEmailAsync(user.Employee.EmailAddress, subject, body);

                TempData["ResetPasswordEmail"] = user.Employee.EmailAddress;
                return Json(new
                {
                    success = true,
                    message = "Đã gửi yêu cầu thành công. Vui lòng kiểm tra email của bạn để nhận hướng dẫn đặt lại mật khẩu!",
                    redirectUrl = Url.Action("ForgetPasswordSent", "Account", new { area = "Admin" })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet("gui-yeu-cau-doi-mat-khau")]
        [AllowAnonymous]
        public IActionResult ForgetPasswordSent()
        {
            var email = TempData["ResetPasswordEmail"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return Redirect("/quen-mat-khau");
            }
            ViewData["Email"] = email;
            return View();
        }

        [HttpGet("dat-lai-mat-khau")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Redirect("/dang-nhap");
            }

            // Decode URL-encoded characters
            code = System.Web.HttpUtility.UrlDecode(code);

            var (isValid, userId, message) = _resetCodeHelper.ValidateResetCode(code);

            if (!isValid || string.IsNullOrEmpty(userId))
            {
                ViewData["Message"] = "Liên kết đặt lại mật khẩu không tồn tại hoặc đã hết hạn.";
                return View("~/Views/Error/Error404.cshtml");
            }

            var user = await _userRepository.GetByUsernameAsync(userId);
            if (user == null || user.Employee == null)
            {
                ViewData["Message"] = "Liên kết đặt lại mật khẩu không tồn tại hoặc đã hết hạn.";
                return View("~/Views/Error/Error404.cshtml");
            }

            // Check if this reset code has been used before
            if (!string.IsNullOrEmpty(user.UsedResetCode) && user.UsedResetCode == code)
            {
                ViewData["Message"] = "Liên kết đặt lại mật khẩu không tồn tại hoặc đã hết hạn.";
                return View("~/Views/Error/Error404.cshtml");
            }

            ViewData["Code"] = code;
            ViewData["Username"] = user.Username;
            ViewData["Email"] = user.Employee.EmailAddress;
            return View();
        }

        [HttpPost("dat-lai-mat-khau")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromForm] string code, [FromForm] string newPassword, [FromForm] string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return Json(new { success = false, message = "Mã reset không hợp lệ!" });
                }

                // Decode URL-encoded characters
                code = System.Web.HttpUtility.UrlDecode(code);

                var (isValid, userId, message) = _resetCodeHelper.ValidateResetCode(code);
                if (!isValid || string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn!" });
                }

                var validationResult = _validator.ValidateResetPassword(newPassword, confirmPassword);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                var user = await _userRepository.GetByUsernameAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng!" });
                }

                // Check if this reset code has been used before
                if (!string.IsNullOrEmpty(user.UsedResetCode) && user.UsedResetCode == code)
                {
                    return NotFound("Liên kết đặt lại mật khẩu không tồn tại hoặc đã hết hạn.");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.UsedResetCode = code; // Store the used reset code
                await _userRepository.UpdateAsync(user);

                return Json(new
                {
                    success = true,
                    message = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập lại.",
                    redirectUrl = "/dang-nhap"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet("tu-choi-truy-cap")]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
