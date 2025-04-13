using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Project.Extensions;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Helpers
{
    public class ViewBagHelper
    {
        private readonly IEmployeeCategoryRepository _employeeCategoryRepository;
        private readonly IMedicineCategoryRepository _medicineCategoryRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ITreatmentMethodRepository _treatmentMethodRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IRegulationRepository _regulationRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;

        public ViewBagHelper(
            IEmployeeCategoryRepository employeeCategoryRepository,
            IMedicineCategoryRepository medicineCategoryRepository,
            IDepartmentRepository departmentRepository,
            ITreatmentMethodRepository treatmentMethodRepository,
            IPatientRepository patientRepository,
            IRoomRepository roomRepository,
            IRegulationRepository regulationRepository,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            JwtManager jwtManager)
        {
            _employeeCategoryRepository = employeeCategoryRepository;
            _medicineCategoryRepository = medicineCategoryRepository;
            _departmentRepository = departmentRepository;
            _treatmentMethodRepository = treatmentMethodRepository;
            _patientRepository = patientRepository;
            _roomRepository = roomRepository;
            _regulationRepository = regulationRepository;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
        }

        public async Task BaseViewBag(ViewDataDictionary viewData, string authToken = null)
        {
            // Lấy thông tin bác sĩ đăng nhập (nếu có token)
            Guid? employeeId = null;
            Guid? departmentId = null;
            if (!string.IsNullOrEmpty(authToken))
            {
                var (username, role) = _jwtManager.GetClaimsFromToken(authToken);
                if (!string.IsNullOrEmpty(username))
                {
                    var user = await _userRepository.GetByUsernameAsync(username);
                    if (user != null && user.Employee != null)
                    {
                        employeeId = user.Employee.Id;
                        departmentId = user.Employee.DepartmentId;
                    }
                }
            }

            viewData["EmployeeId"] = employeeId;
            viewData["DepartmentId"] = departmentId;

            // Danh sách bệnh nhân
            var patients = await _patientRepository.GetAllAsync();
            viewData["Patients"] = patients
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

            // Danh sách danh mục nhân viên
            var employeeCategories = await _employeeCategoryRepository.GetAllAsync();
            viewData["EmployeeCategories"] = employeeCategories
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

            // Danh sách khoa
            var departments = await _departmentRepository.GetAllAsync();
            viewData["Departments"] = departments
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

            // Danh sách danh mục thuốc
            var medicineCategories = await _medicineCategoryRepository.GetAllAsync();
            viewData["MedicineCategories"] = medicineCategories
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

            // Danh sách phương pháp điều trị (lọc theo khoa của bác sĩ nếu có)
            var treatmentMethods = await _treatmentMethodRepository.GetAllAsync();
            if (departmentId.HasValue)
            {
                viewData["TreatmentMethods"] = treatmentMethods
                    .Where(tm => tm.DepartmentId == departmentId && tm.IsActive)
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

            // Danh sách phòng
            var rooms = await _roomRepository.GetAllAsync();
            viewData["Rooms"] = rooms
                .Where(r => r.IsActive)
                .Select(r => new { r.Id, r.Name, r.TreatmentMethodId })
                .ToList();

            // Danh sách quy định có hiệu lực
            var currentDate = DateTime.UtcNow;
            var regulations = await _regulationRepository.GetAllAsync();
            viewData["Regulations"] = regulations
                .Where(r => r.IsActive && r.EffectiveDate <= currentDate && r.ExpirationDate >= currentDate)
                .Select(r => new { r.Id, r.Name })
                .ToList();

            // Các enum
            viewData["GenderOptions"] = Enum.GetValues(typeof(GenderType))
                .Cast<GenderType>()
                .Select(e => new { Value = (int)e, Text = e.GetDisplayName() })
                .ToList();

            viewData["StatusOptions"] = Enum.GetValues(typeof(EmployeeStatus))
                .Cast<EmployeeStatus>()
                .Select(e => new { Value = (int)e, Text = e.GetDisplayName() })
                .ToList();

            viewData["DegreeOptions"] = Enum.GetValues(typeof(DegreeType))
                .Cast<DegreeType>()
                .Select(e => new { Value = (int)e, Text = e.GetDisplayName() })
                .ToList();

            viewData["ProfessionalOptions"] = Enum.GetValues(typeof(ProfessionalQualificationType))
                .Cast<ProfessionalQualificationType>()
                .Select(e => new { Value = (int)e, Text = e.GetDisplayName() })
                .ToList();

            viewData["RoleOptions"] = Enum.GetValues(typeof(RoleType))
                .Cast<RoleType>()
                .Select(e => new { Value = (int)e, Text = e.GetDisplayName() })
                .ToList();

            viewData["StockUnit"] = Enum.GetValues(typeof(UnitType))
                .Cast<UnitType>()
                .Select(e => new { Value = (int)e, Text = e.GetDisplayName() })
                .ToList();

            viewData["TreatmentStatus"] = Enum.GetValues(typeof(TreatmentStatus))
                .Cast<TreatmentStatus>()
                .Select(e => new { Value = (int)e, Text = e.GetDisplayName() })
                .ToList();

            viewData["DiagnosisOptions"] = Enum.GetValues(typeof(DiagnosisType))
                .Cast<DiagnosisType>()
                .Select(e => new { Value = (int)e, Text = e.GetDisplayName() })
                .ToList();

            viewData["ManufacturerOptions"] = Enum.GetValues(typeof(ManufacturerType))
                .Cast<ManufacturerType>()
                .Select(e => new { Value = (int)e, Text = e.GetDisplayName() })
                .ToList();

            viewData["EnumDisplayNames"] = EnumHelper.GetEnumDisplayNames();
        }
    }
}