using Project.Areas.Admin.Models.Entities;
using Project.Repositories;

namespace Project.Services.Implementations
{
    public class MedicineService : IBaseService<Medicine>
    {
        private readonly IBaseRepository<Medicine> _repository;

        public MedicineService(IBaseRepository<Medicine> repository)
        {
            _repository = repository;
        }

        public async Task<bool> IsCodeUniqueAsync(string code, Guid? id = null)
        {
            var existing = await _repository.FirstOrDefaultAsync(m => m.Code == code);
            return existing == null || (id.HasValue && existing.Id == id.Value);
        }

        public async Task<bool> IsNameUniqueAsync(string name, Guid? id = null)
        {
            var existing = await _repository.FirstOrDefaultAsync(m => m.Name == name);
            return existing == null || (id.HasValue && existing.Id == id.Value);
        }
    }
}
