using Project.Repositories.Interfaces;

namespace Project.Helpers
{
    public class CodeGeneratorHelper
    {
        private readonly IEmployeeRepository _employeeRepository;

        public CodeGeneratorHelper(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<string> GenerateUniqueCodeAsync(int length = 8, int maxAttempts = 10)
        {
            if (length <= 0)
            {
                throw new ArgumentException("Độ dài mã phải lớn hơn 0.", nameof(length));
            }

            string randomCode;
            var random = new Random();
            int attempts = 0;

            do
            {
                if (attempts >= maxAttempts)
                {
                    throw new InvalidOperationException("Không thể tạo mã duy nhất sau nhiều lần thử. Vui lòng thử lại.");
                }

                int minValue = (int)Math.Pow(10, length - 1);
                int maxValue = (int)Math.Pow(10, length) - 1;
                randomCode = random.Next(minValue, maxValue + 1).ToString("D" + length);
                attempts++;
            } while (await _employeeRepository.IsCodeExistsAsync(randomCode));

            return randomCode;
        }
    }
}
