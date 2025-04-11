using Microsoft.EntityFrameworkCore;
using Project.Repositories;

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
            if (id.HasValue)
            {
                return !await _repository.AnyAsync(e => EF.Property<string>(e, "Code") == code && EF.Property<Guid>(e, "Id") != id.Value);
            }
            return !await _repository.AnyAsync(e => EF.Property<string>(e, "Code") == code);
        }

        public virtual async Task<bool> IsNumberUniqueAsync(string number, Guid? id = null)
        {
            if (id.HasValue)
            {
                return !await _repository.AnyAsync(e => EF.Property<string>(e, "Number") == number && EF.Property<Guid>(e, "Id") != id.Value);
            }
            return !await _repository.AnyAsync(e => EF.Property<string>(e, "Number") == number);
        }

        public virtual async Task<bool> IsNameUniqueAsync(string name, Guid? id = null)
        {
            if (id.HasValue)
            {
                return !await _repository.AnyAsync(e => EF.Property<string>(e, "Name") == name && EF.Property<Guid>(e, "Id") != id.Value);
            }
            return !await _repository.AnyAsync(e => EF.Property<string>(e, "Name") == name);
        }
    }
}
