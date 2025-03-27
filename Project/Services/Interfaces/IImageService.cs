namespace Project.Services.Interfaces
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile imageFile, string entity);
        void DeleteImage(string imagePath, string entity);
    }
}
