using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Project.Areas.Admin.Models.Enums.Employee;
using Project.Areas.Admin.Models.Enums.Medicines;
using Project.Extensions;
using Project.Repositories.Interfaces;

namespace Project.Helpers
{
    public class ViewBagHelper
    {
        private readonly IEmployeeCategoryRepository _employeecategoryRepository;
        private readonly IMedicineCategoryRepository _medicinecategoryRepository;
        private readonly IDepartmentRepository _depRepository;
        private readonly ITreatmentMethodRepository _treatmentRepository;
        private readonly CodeGeneratorHelper _codeGenHelper;
        public ViewBagHelper
        (
            IEmployeeCategoryRepository employeecategoryRepository,
            IMedicineCategoryRepository medicinecategoryRepository,
            IDepartmentRepository depRepository,
            ITreatmentMethodRepository treatmentRepository,
            CodeGeneratorHelper codeGenHelper

        )
        {
            _employeecategoryRepository = employeecategoryRepository;
            _medicinecategoryRepository = medicinecategoryRepository;
            _depRepository = depRepository;
            _treatmentRepository = treatmentRepository;
            _codeGenHelper = codeGenHelper;
        }

        public async Task BaseViewBag(ViewDataDictionary viewData)
        {
            var employeeCategories = await _employeecategoryRepository.GetAllAsync();
            viewData["EmployeeCategories"] = employeeCategories
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

            var departments = await _depRepository.GetAllAsync();
            viewData["Departments"] = departments
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

            var medicineCategories = await _medicinecategoryRepository.GetAllAsync();
            viewData["MedicineCategories"] = medicineCategories
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

            var treatments = await _treatmentRepository.GetAllAsync();
            viewData["Treatments"] = treatments
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

            viewData["GenderOptions"] = Enum.GetValues(typeof(GenderType))
                .Cast<GenderType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();

            viewData["StatusOptions"] = Enum.GetValues(typeof(EmployeeStatus))
                .Cast<EmployeeStatus>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();

            viewData["DegreeOptions"] = Enum.GetValues(typeof(DegreeType))
                .Cast<DegreeType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();

            viewData["ProfessionalOptions"] = Enum.GetValues(typeof(ProfessionalQualificationType))
                .Cast<ProfessionalQualificationType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();

            viewData["RoleOptions"] = Enum.GetValues(typeof(RoleType))
                .Cast<RoleType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();

            viewData["StockUnit"] = Enum.GetValues(typeof(UnitType))
                .Cast<UnitType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();


            string randomCode = await _codeGenHelper.GenerateUniqueCodeAsync();
            viewData["RandomCode"] = randomCode;
        }
    }
}
