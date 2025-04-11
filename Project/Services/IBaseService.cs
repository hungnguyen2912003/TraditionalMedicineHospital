namespace Project.Services
{
    public interface IBaseService
    {
        Task<bool> IsCodeUniqueAsync(string code, Guid? id = null);
        Task<bool> IsNameUniqueAsync(string name, Guid? id = null);
        Task<bool> IsNumberUniqueAsync(string number, Guid? id = null);
    }
    public interface IBaseService<T> : IBaseService where T : class
    {
    }
}
