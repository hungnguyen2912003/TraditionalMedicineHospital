using Project.Models.Commons;
using Project.Repositories;

namespace Project.Helpers
{
    public class CodeGeneratorHelper
    {
        private const string DefaultChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const string NumbersOnlyChars = "0123456789";

        public async Task<string> GenerateUniqueCodeAsync<T>(
            IBaseRepository<T> repository,
            int length = 8,
            string chars = DefaultChars) where T : class,
            ICodeEntity
        {
            if (string.IsNullOrEmpty(chars))
            {
                throw new ArgumentException("Tập hợp ký tự không được rỗng.", nameof(chars));
            }

            var random = new Random();
            string code;

            do
            {
                code = new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            } while (await repository.IsCodeExistsAsync(code));

            return code;
        }

        public async Task<string> GenerateNumericCodeAsync<T>(
            IBaseRepository<T> repository,
            int length = 8) where T : class, ICodeEntity
        {
            return await GenerateUniqueCodeAsync(repository, length, NumbersOnlyChars);
        }
    }
}
