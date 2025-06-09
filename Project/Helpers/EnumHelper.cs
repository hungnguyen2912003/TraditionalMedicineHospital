using Project.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Project.Helpers
{
    public class EnumHelper
    {
        public static Dictionary<string, Dictionary<int, string>> GetEnumDisplayNames()
        {
            var enumTypes = new[] {
                typeof(GenderType),
                typeof(DiagnosisType),
                typeof(DegreeType),
                typeof(ProfessionalQualificationType),
                typeof(EmployeeStatus),
                typeof(ManufacturerType),
                typeof(RoleType),
                typeof(TreatmentStatus),
                typeof(UnitType),
                typeof(TrackingStatus),
                typeof(RoomType),
                typeof(PaymentStatus),
                typeof(HealthInsuranceRegistrationPlace),
                typeof(PaymentType)
            };
            var result = new Dictionary<string, Dictionary<int, string>>();

            foreach (var enumType in enumTypes)
            {
                var enumName = enumType.Name;
                var enumValues = new Dictionary<int, string>();

                foreach (var value in Enum.GetValues(enumType))
                {
                    var memberInfo = enumType.GetMember(value.ToString() ?? string.Empty).FirstOrDefault();
                    var displayAttribute = memberInfo?.GetCustomAttribute<DisplayAttribute>();
                    var displayName = displayAttribute?.Name ?? value.ToString();
                    enumValues[(int)value] = displayName ?? string.Empty;
                }

                result[enumName] = enumValues;
            }

            return result;
        }
    }
}
