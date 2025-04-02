using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Data;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<Department?> GetByCodeAsync(string code)
        {
            return await _context.departments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<Department?> GetByNameAsync(string name)
        {
            return await _context.departments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}
