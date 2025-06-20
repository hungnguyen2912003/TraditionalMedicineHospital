﻿using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class RoomRepository : BaseRepository<Room>, IRoomRepository
    {
        public RoomRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Room>> GetRoomsByTreatmentMethodAsync(Guid id)
        {
            return await _context.rooms
                .Include(r => r.TreatmentMethod)
                .Where(r => r.TreatmentMethodId == id)
                .Select(r => new Room
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    TreatmentMethodId = r.TreatmentMethodId,
                    TreatmentMethod = r.TreatmentMethod,
                    DepartmentId = r.DepartmentId,
                    Department = r.Department
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetRoomsByDepartmentAsync(Guid departmentId)
        {
            return await _context.rooms
                .Include(r => r.TreatmentMethod)
                .Where(r => r.DepartmentId == departmentId)
                .Select(r => new Room
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    TreatmentMethodId = r.TreatmentMethodId,
                    TreatmentMethod = r.TreatmentMethod,
                    DepartmentId = r.DepartmentId,
                    Department = r.Department
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetAllAdvancedAsync()
        {
            return await _context.rooms
                .Include(m => m.TreatmentMethod)
                .Include(m => m.Department)
                .Include(m => m.Employees)
                .ToListAsync();
        }

        public async Task<Room?> GetByIdAdvancedAsync(Guid id)
        {
            return await _context.rooms
                .Include(m => m.TreatmentMethod)
                .Include(m => m.Department)
                .Include(m => m.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
