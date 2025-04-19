namespace Project.Validators
{
    public class AuthValidator
    {
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }

            public ValidationResult(bool isValid, string errorMessage = "")
            {
                IsValid = isValid;
                ErrorMessage = errorMessage;
            }
        }

        public ValidationResult ValidateLogin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult(false, "Mã nhân viên và mật khẩu không được bỏ trống!");
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                return new ValidationResult(false, "Mã nhân viên không được bỏ trống!");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult(false, "Mật khẩu không được bỏ trống!");
            }

            return new ValidationResult(true);
        }

        public ValidationResult ValidateChangePassword(string oldPassword, string newPassword, string confirmPassword, string currentPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                return new ValidationResult(false, "Mật khẩu cũ không được bỏ trống!");
            }
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return new ValidationResult(false, "Mật khẩu mới không được bỏ trống!");
            }
            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                return new ValidationResult(false, "Xác nhận mật khẩu không được bỏ trống!");
            }

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, currentPasswordHash))
            {
                return new ValidationResult(false, "Mật khẩu cũ không đúng!");
            }

            if (newPassword.Length < 6)
            {
                return new ValidationResult(false, "Mật khẩu mới phải có ít nhất 6 ký tự!");
            }

            if (newPassword != confirmPassword)
            {
                return new ValidationResult(false, "Mật khẩu xác nhận không khớp!");
            }

            if (oldPassword == newPassword)
            {
                return new ValidationResult(false, "Mật khẩu mới không được trùng với mật khẩu cũ!");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(newPassword, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?])[A-Za-z\d!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]{6,}$"))
            {
                return new ValidationResult(false, "Mật khẩu mới phải chứa ít nhất 1 chữ cái hoa, chữ cái thường, chữ số và ký tự đặc biệt!");
            }

            return new ValidationResult(true);
        }

        public ValidationResult ValidateForgotPassword(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new ValidationResult(false, "Mã nhân viên không được bỏ trống!");
            }

            return new ValidationResult(true);
        }

        public ValidationResult ValidateResetPassword(string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                return new ValidationResult(false, "Mật khẩu mới không được bỏ trống!");
            }

            if (newPassword.Length < 6)
            {
                return new ValidationResult(false, "Mật khẩu mới phải có ít nhất 6 ký tự!");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(newPassword, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?])[A-Za-z\d!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]{6,}$"))
            {
                return new ValidationResult(false, "Mật khẩu mới phải chứa ít nhất 1 chữ cái hoa, chữ cái thường, chữ số và ký tự đặc biệt!");
            }

            if (string.IsNullOrEmpty(confirmPassword))
            {
                return new ValidationResult(false, "Xác nhận mật khẩu không được bỏ trống!");
            }

            if (newPassword != confirmPassword)
            {
                return new ValidationResult(false, "Mật khẩu xác nhận không khớp!");
            }

            return new ValidationResult(true);
        }
    }
}
