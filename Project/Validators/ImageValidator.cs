using Microsoft.AspNetCore.Hosting;

namespace Project.Validators
{
    public class ImageValidator
    {
        private readonly IWebHostEnvironment _environment;

        public ImageValidator(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile, string entity)
        {
            // Kiểm tra định dạng
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(imageFile.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception("Định dạng file không hợp lệ. Chỉ chấp nhận .jpg, .jpeg, .png, .gif.");
            }

            // Kiểm tra kích thước (tối đa 5MB)
            if (imageFile.Length > 5 * 1024 * 1024)
            {
                throw new Exception("Kích thước file vượt quá 5MB.");
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(_environment.WebRootPath, entity, fileName);
            var directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath == null)
            {
                throw new Exception("Đường dẫn file không hợp lệ.");
            }
            Directory.CreateDirectory(directoryPath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/{entity}/{fileName}";
        }

        public void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
