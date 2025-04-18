using Project.Areas.Admin.Models.Entities;
using Project.Repositories;
using Project.Services.Interfaces;
using System.Linq.Expressions;

namespace Project.Services.Implementations
{
    public class EmployeeService : BaseService<Employee>, IEmployeeService
    {
        private readonly IBaseRepository<Employee> _repository;

        public EmployeeService(IBaseRepository<Employee> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public override async Task<bool> IsEmailUniqueAsync(string email, Guid? id = null)
        {
            Expression<Func<Employee, bool>> predicate = id.HasValue
                ? e => e.EmailAddress.ToLower() == email.ToLower() && e.Id != id.Value
                : e => e.EmailAddress.ToLower() == email.ToLower();

            return !await _repository.AnyAsync(predicate);
        }

        public override async Task<bool> IsPhoneUniqueAsync(string phone, Guid? id = null)
        {
            Expression<Func<Employee, bool>> predicate = id.HasValue
                ? e => e.PhoneNumber == phone && e.Id != id.Value
                : e => e.PhoneNumber == phone;

            return !await _repository.AnyAsync(predicate);
        }

        public override async Task<bool> IsIdentityNumberUniqueAsync(string identityNumber, Guid? id = null)
        {
            Expression<Func<Employee, bool>> predicate = id.HasValue
                ? e => e.IdentityNumber == identityNumber && e.Id != id.Value
                : e => e.IdentityNumber == identityNumber;

            return !await _repository.AnyAsync(predicate);
        }

        public override async Task<bool> IsCodeUniqueAsync(string code, Guid? id = null)
        {
            Expression<Func<Employee, bool>> predicate = id.HasValue
                ? e => e.Code == code && e.Id != id.Value
                : e => e.Code == code;

            return !await _repository.AnyAsync(predicate);
        }

        public override async Task<bool> IsNameUniqueAsync(string name, Guid? id = null)
        {
            Expression<Func<Employee, bool>> predicate = id.HasValue
                ? e => e.Name == name && e.Id != id.Value
                : e => e.Name == name;

            return !await _repository.AnyAsync(predicate);
        }

        public override Task<bool> IsNumberUniqueAsync(string number, Guid? id = null)
        {
            throw new NotSupportedException("Number uniqueness check is not supported for Employee.");
        }
    }
}