using Project.Areas.Admin.Models.Entities;
using Project.Repositories;

namespace Project.Services.Implementations
{
    public class EmployeeCategoryService : IBaseService<EmployeeCategory>
    {
        private readonly IBaseRepository<EmployeeCategory> _repository;

        public EmployeeCategoryService(IBaseRepository<EmployeeCategory> repository)
        {
            _repository = repository;
        }

        public async Task<bool> IsCodeUniqueAsync(string code, Guid? id = null)
        {
            var existing = await _repository.FirstOrDefaultAsync(mc => mc.Code == code);
            return existing == null || (id.HasValue && existing.Id == id.Value);
        }

        public async Task<bool> IsNameUniqueAsync(string name, Guid? id = null)
        {
            var existing = await _repository.FirstOrDefaultAsync(mc => mc.Name == name);
            return existing == null || (id.HasValue && existing.Id == id.Value);
        }

        public Task<bool> IsNumberUniqueAsync(string number, Guid? id = null)
        {
            throw new NotImplementedException();
        }
    }
}
