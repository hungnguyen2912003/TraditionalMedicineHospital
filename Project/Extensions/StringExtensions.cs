using System;
using System.Text.RegularExpressions;

namespace Project.Extensions
{
    public static class StringExtensions
    {
        public static string MaskEmail(this string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            var parts = email.Split('@');
            if (parts.Length != 2)
                return email;

            var name = parts[0];
            var domain = parts[1];

            // Giữ lại 6-7 ký tự cuối của phần tên, che phần còn lại bằng dấu *
            var visibleChars = Math.Min(7, Math.Max(6, name.Length / 2)); // Giữ lại 6-7 ký tự hoặc nửa độ dài nếu tên ngắn
            var maskedName = name.Length <= visibleChars
                ? new string('*', name.Length)
                : new string('*', name.Length - visibleChars) + name.Substring(name.Length - visibleChars);

            return $"{maskedName}@{domain}";
        }
    }
}