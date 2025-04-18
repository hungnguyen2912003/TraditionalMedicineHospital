using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Project.Repositories;
using System.Linq.Expressions;

namespace Project.Services
{
    public class BaseService<T> : IBaseService<T> where T : class
    {
        private readonly IBaseRepository<T> _repository;

        public BaseService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual async Task<bool> IsCodeUniqueAsync(string code, Guid? id = null)
        {
            Expression<Func<T, bool>> predicate = id.HasValue
                ? e => EF.Property<string>(e, "Code") == code && EF.Property<Guid>(e, "Id") != id.Value
                : e => EF.Property<string>(e, "Code") == code;

            return !await _repository.AnyAsync(predicate);
        }

        public virtual async Task<bool> IsNumberUniqueAsync(string number, Guid? id = null)
        {
            Expression<Func<T, bool>> predicate = id.HasValue
                ? e => EF.Property<string>(e, "Number") == number && EF.Property<Guid>(e, "Id") != id.Value
                : e => EF.Property<string>(e, "Number") == number;

            return !await _repository.AnyAsync(predicate);
        }

        public virtual async Task<bool> IsNameUniqueAsync(string name, Guid? id = null)
        {
            Expression<Func<T, bool>> predicate = id.HasValue
                ? e => EF.Property<string>(e, "Name") == name && EF.Property<Guid>(e, "Id") != id.Value
                : e => EF.Property<string>(e, "Name") == name;

            return !await _repository.AnyAsync(predicate);
        }

        public virtual async Task<bool> IsEmailUniqueAsync(string email, Guid? id = null)
        {
            Expression<Func<T, bool>> predicate = id.HasValue
                ? e => EF.Property<string>(e, "EmailAddress").ToLower() == email.ToLower() && EF.Property<Guid>(e, "Id") != id.Value
                : e => EF.Property<string>(e, "EmailAddress").ToLower() == email.ToLower();

            return !await _repository.AnyAsync(predicate);
        }

        public virtual async Task<bool> IsPhoneUniqueAsync(string phone, Guid? id = null)
        {
            Expression<Func<T, bool>> predicate = id.HasValue
                ? e => EF.Property<string>(e, "PhoneNumber") == phone && EF.Property<Guid>(e, "Id") != id.Value
                : e => EF.Property<string>(e, "PhoneNumber") == phone;

            return !await _repository.AnyAsync(predicate);
        }

        public virtual async Task<bool> IsIdentityNumberUniqueAsync(string identityNumber, Guid? id = null)
        {
            Expression<Func<T, bool>> predicate = id.HasValue
                ? e => EF.Property<string>(e, "IdentityNumber") == identityNumber && EF.Property<Guid>(e, "Id") != id.Value
                : e => EF.Property<string>(e, "IdentityNumber") == identityNumber;

            return !await _repository.AnyAsync(predicate);
        }

        public virtual async Task<bool> IsNumberHealthInsuranceUniqueAsync(string numberHealthInsurance, Guid? id = null)
        {
            Expression<Func<T, bool>> predicate = id.HasValue
                ? e => EF.Property<string>(e, "Number") == numberHealthInsurance && EF.Property<Guid>(e, "Id") != id.Value
                : e => EF.Property<string>(e, "Number") == numberHealthInsurance;

            return !await _repository.AnyAsync(predicate);
        }
    }
}
