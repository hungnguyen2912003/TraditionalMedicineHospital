namespace Project.Services
{
    public interface IBaseService
    {
        Task<bool> IsCodeUniqueAsync(string code, Guid? id = null);
        Task<bool> IsNameUniqueAsync(string name, Guid? id = null);
        Task<bool> IsNumberUniqueAsync(string number, Guid? id = null);
        Task<bool> IsEmailUniqueAsync(string email, Guid? id = null);
        Task<bool> IsPhoneUniqueAsync(string phone, Guid? id = null);
        Task<bool> IsIdentityNumberUniqueAsync(string identityNumber, Guid? id = null);
    }
    public interface IBaseService<T> : IBaseService where T : class
    {
    }
}
