namespace Project.Validators
{
    public class AuthValidator
    {
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }

            public ValidationResult(bool isValid, string errorMessage = null)
            {
                IsValid = isValid;
                ErrorMessage = errorMessage;
            }
        }

        public ValidationResult ValidateLogin(string code, string password)
        {
            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult(false, "Mã nhân viên và mật khẩu không được bỏ trống!");
            }
            if (string.IsNullOrWhiteSpace(code))
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
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                return new ValidationResult(false, "Mật khẩu cũ, mật khẩu mới và xác nhận mật khẩu không được bỏ trống!");
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

            return new ValidationResult(true);
        }

        public ValidationResult ValidateForgotPassword(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new ValidationResult(false, "Mã nhân viên không được bỏ trống!");
            }

            return new ValidationResult(true);
        }
    }
}
