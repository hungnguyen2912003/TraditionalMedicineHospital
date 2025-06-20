﻿using Project.Areas.Admin.Models.Entities;

namespace Project.Repositories.Interfaces
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        Task<IEnumerable<Employee>> GetAllAdvancedAsync();
        Task<Employee?> GetByIdAdvancedAsync(Guid id);
        Task<Employee?> GetByUsernameAsync(string username);
        Task<IEnumerable<Employee>> GetByCodesAsync(IEnumerable<string> codes);
        Task<string?> GetRoomNameByEmployeeIdAsync(Guid employeeId);
        Task<IEnumerable<Employee>> GetByIdsAsync(List<Guid> ids);
    }
}
