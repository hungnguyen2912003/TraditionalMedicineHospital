using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Project.Extensions;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Helpers
{
    public class ViewBagHelper
    {
        private readonly IEmployeeCategoryRepository _employeecategoryRepository;
        private readonly IMedicineCategoryRepository _medicinecategoryRepository;
        private readonly IDepartmentRepository _depRepository;
        private readonly ITreatmentMethodRepository _treatmentRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly CodeGeneratorHelper _codeGenHelper;
        private readonly IUserRepository _userRepository;
        public readonly JwtManager _jwtManager;
        private readonly IRoomRepository _roomRepository;
        private readonly IRegulationRepository _regulationRepository;
        public ViewBagHelper
        (
            IEmployeeCategoryRepository employeecategoryRepository,
            IMedicineCategoryRepository medicinecategoryRepository,
            IDepartmentRepository depRepository,
            ITreatmentMethodRepository treatmentRepository,
            IPatientRepository patientRepository,
            CodeGeneratorHelper codeGenHelper,
            IUserRepository userRepository,
            JwtManager jwtManager,
            IRoomRepository roomRepository,
            IRegulationRepository regulationRepository

        )
        {
            _employeecategoryRepository = employeecategoryRepository;
            _medicinecategoryRepository = medicinecategoryRepository;
            _depRepository = depRepository;
            _treatmentRepository = treatmentRepository;
            _patientRepository = patientRepository;
            _codeGenHelper = codeGenHelper;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
            _roomRepository = roomRepository;
            _regulationRepository = regulationRepository;
        }

        public async Task BaseViewBag(ViewDataDictionary viewData, string authToken = null)
        {
            // Get current user
            Guid? userId = null;
            Guid? depId = null;
            if (!string.IsNullOrEmpty(authToken))
            {
                var (username, role) = _jwtManager.GetClaimsFromToken(authToken);
                if (!string.IsNullOrEmpty(username))
                {
                    var user = await _userRepository.GetByUsernameAsync(username);
                    if (user != null && user.Employee != null)
                    {
                        userId = user.Employee.Id;
                        depId = user.Employee.DepartmentId;
                    }
                }
            }

            viewData["UserId"] = userId;
            viewData["DepId"] = depId;

            var patients = await _patientRepository.GetAllAsync();
            viewData["Patients"] = patients
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

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

            var treatmentMethods = await _treatmentRepository.GetAllAsync();
            if (depId.HasValue)
            {
                viewData["TreatmentMethods"] = treatmentMethods
                    .Where(tm => tm.IsActive && tm.DepartmentId == depId)
                    .Select(tm => new { tm.Id, tm.Name })
                    .ToList();
            }
            else
            {
                viewData["TreatmentMethods"] = treatmentMethods
                    .Where(tm => tm.IsActive)
                    .Select(tm => new { tm.Id, tm.Name })
                    .ToList();
            }

            var rooms = await _roomRepository.GetAllAsync();
            viewData["Rooms"] = rooms
                .Where(r => r.IsActive)
                .Select(r => new { r.Id, r.Name, r.TreatmentMethodId })
                .ToList();

            var currentDate = DateTime.Now;
            var regulations = await _regulationRepository.GetAllAsync();
            viewData["Regulations"] = regulations
                .Where(r => r.IsActive && r.EffectiveDate <= currentDate && r.ExpirationDate >= currentDate)
                .Select(r => new { r.Id, r.Name })
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

            viewData["TreatmentStatus"] = Enum.GetValues(typeof(TreatmentStatus))
                .Cast<TreatmentStatus>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();

            viewData["DiagnosisOptions"] = Enum.GetValues(typeof(DiagnosisType))
                .Cast<DiagnosisType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();

            viewData["ManufacturerOptions"] = Enum.GetValues(typeof(ManufacturerType))
                .Cast<ManufacturerType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();

            viewData["EnumDisplayNames"] = EnumHelper.GetEnumDisplayNames();
        }
    }
}
