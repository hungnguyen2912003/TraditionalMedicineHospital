using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        protected Task<bool> BeAValidDate(DateTime date)
        {
            return Task.FromResult(date != DateTime.MinValue);
        }

        protected Task<bool> BeAfterManufacturedDate(DateTime manufacturedDate, DateTime expiryDate)
        {
            if (manufacturedDate == DateTime.MinValue || expiryDate == DateTime.MinValue)
                return Task.FromResult(true);
            return Task.FromResult(expiryDate > manufacturedDate);
        }
    }
}