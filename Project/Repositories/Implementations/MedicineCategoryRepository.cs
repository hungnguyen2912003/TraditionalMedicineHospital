﻿using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;
using Project.Datas;
using Project.Repositories.Interfaces;

namespace Project.Repositories.Implementations
{
    public class MedicineCategoryRepository : BaseRepository<MedicineCategory>, IMedicineCategoryRepository
    {
        public MedicineCategoryRepository(TraditionalMedicineHospitalDbContext context) : base(context)
        {
        }
    }
}
