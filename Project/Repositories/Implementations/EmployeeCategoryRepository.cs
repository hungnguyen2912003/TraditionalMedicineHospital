using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Data;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class EmployeeCategoryRepository : BaseRepository<EmployeeCategory>, IEmployeeCategoryRepository
    {
        public EmployeeCategoryRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<EmployeeCategory?> GetByCodeAsync(string code)
        {
            return await _context.employeeCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<EmployeeCategory?> GetByNameAsync(string name)
        {
            return await _context.employeeCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}
