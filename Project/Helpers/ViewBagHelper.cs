﻿using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        private readonly IHealthInsuranceRepository _healthInsuranceRepository;
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
            IRegulationRepository regulationRepository,
            IHealthInsuranceRepository healthInsuranceRepository
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
            _healthInsuranceRepository = healthInsuranceRepository;
        }

        public async Task BaseViewBag(ViewDataDictionary viewData, string? authToken = null)
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

            if (!string.IsNullOrEmpty(authToken))
            {
                var (username, role) = _jwtManager.GetClaimsFromToken(authToken);
                if (!string.IsNullOrEmpty(username))
                {
                    var user = await _userRepository.GetByUsernameAsync(username);
                    if (user != null && user.Employee != null)
                    {
                        viewData["EmployeeName"] = user.Employee.Name;
                    }
                }
            }

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
                viewData["TreatmentMethods_Reception"] = treatmentMethods
                    .Where(tm => tm.IsActive && tm.DepartmentId == depId)
                    .Select(tm => new { tm.Id, tm.Name })
                    .ToList();
            }

            viewData["TreatmentMethods"] = treatmentMethods
                .Where(tm => tm.IsActive)
                .Select(tm => new { tm.Id, tm.Name })
                .ToList();

            var rooms = await _roomRepository.GetAllAsync();
            viewData["Rooms"] = rooms
                .Where(r => r.IsActive)
                .Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    treatmentMethodId = r.TreatmentMethodId
                })
                .ToList();

            var currentDate = DateTime.Now;
            var regulations = await _regulationRepository.GetAllAsync();
            viewData["Regulations"] = regulations
                .Where(r => r.IsActive && r.ExpirationDate >= currentDate)
                .Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    effectiveStartDate = r.EffectiveDate.ToString("dd/MM/yyyy"),
                    effectiveEndDate = r.ExpirationDate.ToString("dd/MM/yyyy")
                })
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

        public async Task GetPatientsWithoutInsurance(ViewDataDictionary viewData)
        {
            // Get all active patients
            var allPatients = await _patientRepository.GetAllAdvancedAsync();
            var activePatients = allPatients.Where(x => x.IsActive == true);

            // Get all active health insurances
            var allHealthInsurances = await _healthInsuranceRepository.GetAllAdvancedAsync();
            var activeHealthInsurances = allHealthInsurances.Where(x => x.IsActive == true);

            // Get patients who don't have health insurance
            var patientsWithoutInsurance = activePatients.Where(p =>
                !activeHealthInsurances.Any(hi => hi.PatientId == p.Id));

            viewData["PatientsWithoutInsurance"] = patientsWithoutInsurance;
        }
    }
}
