using Project.Services.Interfaces;

namespace Project.Services.Implementations
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".jfif" };
        private const long _maxFileSize = 5 * 1024 * 1024;
        private const string _imageFolder = "Images";

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public async Task<string> SaveImageAsync(IFormFile imageFile, string entity)
        {
            if (imageFile == null)
            {
                throw new ArgumentNullException(nameof(imageFile), "File hình ảnh không được null.");
            }

            ValidateFileExtension(imageFile);

            ValidateFileSize(imageFile);

            var fileName = GenerateFileName(imageFile);
            var filePath = GenerateFilePath(entity, fileName);

            await SaveFileAsync(imageFile, filePath);

            return fileName;
        }

        public void DeleteImage(string imagePath, string entity)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            var fullPath = Path.Combine(_environment.WebRootPath, _imageFolder, entity, imagePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        private void ValidateFileExtension(IFormFile imageFile)
        {
            var extension = Path.GetExtension(imageFile.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new Exception("Định dạng file không hợp lệ. Chỉ chấp nhận .jpg, .jpeg, .png, .gif, .jfif.");
            }
        }

        private void ValidateFileSize(IFormFile imageFile)
        {
            if (imageFile.Length > _maxFileSize)
            {
                throw new Exception("Kích thước file vượt quá 5MB.");
            }
        }

        private string GenerateFileName(IFormFile imageFile)
        {
            var extension = Path.GetExtension(imageFile.FileName).ToLower();
            return Guid.NewGuid().ToString() + extension;
        }

        private string GenerateFilePath(string entity, string fileName)
        {
            var filePath = Path.Combine(_environment.WebRootPath, _imageFolder, entity, fileName);
            var directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath == null)
            {
                throw new Exception("Đường dẫn file không hợp lệ.");
            }
            Directory.CreateDirectory(directoryPath);
            return filePath;
        }

        private async Task SaveFileAsync(IFormFile imageFile, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
        }
    }
}
