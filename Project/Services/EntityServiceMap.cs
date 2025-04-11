using Project.Areas.Admin.Models.Entities;
using Project.Areas.Staff.Models.Entities;

namespace Project.Services
{
    public static class EntityServiceMap
    {
        public static readonly Dictionary<string, Type> ServiceTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "medicine", typeof(IBaseService<Medicine>) },
            { "medicinecategory", typeof(IBaseService<MedicineCategory>) },
            { "department", typeof(IBaseService<Department>) },
            { "employeecategory", typeof(IBaseService<EmployeeCategory>) },
            { "employee", typeof(IBaseService<Employee>) },
            { "treatment", typeof(IBaseService<TreatmentMethod>) },
            { "room", typeof(IBaseService<Room>) },
            { "regulation", typeof(IBaseService<Regulation>) },
            { "patient", typeof(IBaseService<Patient>) },
            { "treatmentrecord", typeof(IBaseService<TreatmentRecord>) },
            { "healthinsurance", typeof(IBaseService<HealthInsurance>) }
        };
    }
}
