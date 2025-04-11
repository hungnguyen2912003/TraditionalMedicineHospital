using Project.Repositories;

namespace Project.Helpers
{
    public class CodeGeneratorHelper
    {
        public async Task<string> GenerateUniqueCodeAsync<T>(IBaseRepository<T> repository, int length = 8) where T : class
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;

            do
            {
                code = new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            } while (await repository.IsCodeExistsAsync(code));

            return code;
        }
    }
}
