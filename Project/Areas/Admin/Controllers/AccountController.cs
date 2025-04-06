using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Services;
using Project.Validators;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly IEmployeeRepository _repository;
        private readonly AuthValidator _validator;
        private readonly JwtManager _jwtManager;

        public AccountController(IEmployeeRepository repository, AuthValidator validator, JwtManager jwtManager)
        {
            _repository = repository;
            _validator = validator;
            _jwtManager = jwtManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string code, string password)
        {
            try
            {
                var validationResult = _validator.ValidateLogin(code, password);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                var employee = await _repository.GetByCodeAsync(code);
                if (employee != null && BCrypt.Net.BCrypt.Verify(password, employee.PasswordHash))
                {
                    var token = _jwtManager.GenerateToken(code);

                    Response.Cookies.Append("AuthToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                    });

                    if (employee.IsFirstLogin)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Đây là lần đăng nhập đầu tiên của bạn. Vui lòng đổi mật khẩu!",
                            redirectUrl = Url.Action("ChangePassword", "Account", new { area = "Admin" })
                        });
                    }
                    return Json(new
                    {
                        success = true,
                        message = "Đăng nhập thành công!",
                        redirectUrl = Url.Action("Index", "Home", new { area = "Admin" })
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

            var employeeCode = _jwtManager.GetEmployeeCodeFromToken(token);
            if (string.IsNullOrEmpty(employeeCode))
            {
                Response.Cookies.Delete("AuthToken");
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            ViewData["EmployeeCode"] = employeeCode;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Phiên làm việc không hợp lệ. Vui lòng đăng nhập lại.",
                        redirectUrl = Url.Action("Login", "Account", new { area = "Admin" })
                    });
                }

                var code = _jwtManager.GetEmployeeCodeFromToken(token);
                if (string.IsNullOrEmpty(code))
                {
                    Response.Cookies.Delete("AuthToken");
                    return Json(new
                    {
                        success = false,
                        message = "Phiên làm việc không hợp lệ. Vui lòng đăng nhập lại.",
                        redirectUrl = Url.Action("Login", "Account", new { area = "Admin" })
                    });
                }

                var employee = await _repository.GetByCodeAsync(code);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên." });
                }

                var validationResult = _validator.ValidateChangePassword(oldPassword, newPassword, confirmPassword, employee.PasswordHash);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                employee.IsFirstLogin = false;
                await _repository.UpdateAsync(employee);

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

        [HttpPost]
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

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string code)
        {
            try
            {
                var validationResult = _validator.ValidateForgotPassword(code);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                var employee = await _repository.GetByCodeAsync(code);
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

    }
}
