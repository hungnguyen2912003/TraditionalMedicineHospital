using FluentValidation;
using System.Globalization;

namespace Project.Validators
{
    public abstract class BaseUniqueValidator<TDto, TEntity> : AbstractValidator<TDto>
        where TDto : class
        where TEntity : class
    {
        protected readonly Guid? _currentId;

        protected BaseUniqueValidator(Guid? currentId = null)
        {
            _currentId = currentId;
        }

        protected async Task<bool> BeUniqueAsync<TValue>(TDto dto, TValue value, Func<TValue, Task<TEntity>> getByValueAsync, CancellationToken ct)
        {
            var existingEntity = await getByValueAsync(value);
            if (existingEntity == null)
            {
                return true;
            }

            if (_currentId.HasValue)
            {
                var existingId = existingEntity.GetType().GetProperty("Id")?.GetValue(existingEntity)?.ToString();
                return existingId == _currentId.ToString();
            }

            return false;
        }

        protected async Task<bool> BeAValidDate(DateTime date, CancellationToken cancellationToken)
        {
            return await Task.FromResult(date != DateTime.MinValue);
        }

        protected async Task<bool> BeAfterManufacturedDate(DateTime manufacturedDate, DateTime expiryDate)
        {
            if (manufacturedDate == DateTime.MinValue || expiryDate == DateTime.MinValue)
                return await Task.FromResult(true);
            return await Task.FromResult(expiryDate > manufacturedDate);
        }

        protected async Task<bool> BeOver18YearsOld(DateTime dateOfBirth)
        {
            var today = DateTime.UtcNow;
            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return await Task.FromResult(age >= 18);
        }
    }
}