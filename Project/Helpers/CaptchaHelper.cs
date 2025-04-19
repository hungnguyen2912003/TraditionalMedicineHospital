using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Project.Helpers
{
    public class CaptchaHelper
    {
        private const string CAPTCHA_KEY = "CaptchaCode";

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public IActionResult GetCaptchaImage(HttpContext httpContext)
        {
            int width = 100;
            int height = 36;
            var captchaCode = GenerateCaptchaCode();
            var result = GenerateCaptchaImage(width, height, captchaCode);

            httpContext.Session.SetString(CAPTCHA_KEY, captchaCode);

            return result.CaptchaBytes != null
                ? new FileContentResult(result.CaptchaBytes, "image/png")
                : new NotFoundResult();
        }

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        private CaptchaResult GenerateCaptcha(int width, int height, string captchaCode)
        {
            using (Bitmap baseMap = new Bitmap(width, height))
            using (Graphics graph = Graphics.FromImage(baseMap))
            {
                graph.Clear(Color.White);

                Random random = new Random();

                // Add noise
                for (int i = 0; i < (width * height) / 30; i++)
                {
                    int x = random.Next(width);
                    int y = random.Next(height);
                    baseMap.SetPixel(x, y, Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                }

                // Add the text
                using (Font font = new Font("Arial", 18, FontStyle.Bold))
                {
                    using (GraphicsPath textPath = new GraphicsPath())
                    {
                        textPath.AddString(captchaCode, font.FontFamily, (int)font.Style,
                            font.Size, new Point(5, 5), StringFormat.GenericDefault);

                        using (Pen pen = new Pen(Color.DarkBlue))
                        {
                            graph.DrawPath(pen, textPath);
                            graph.FillPath(Brushes.DarkBlue, textPath);
                        }
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    baseMap.Save(ms, ImageFormat.Png);
                    return new CaptchaResult { CaptchaBytes = ms.ToArray() };
                }
            }
        }

        private string GenerateCaptchaCode()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        private CaptchaResult GenerateCaptchaImage(int width, int height, string captchaCode)
        {
            return GenerateCaptcha(width, height, captchaCode);
        }
    }

    public class CaptchaResult
    {
        public byte[]? CaptchaBytes { get; set; }
    }
}