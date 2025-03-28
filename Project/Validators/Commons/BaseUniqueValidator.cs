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
        protected Guid? CurrentId { get; }

        protected BaseUniqueValidator(Guid? currentId = null)
        {
            CurrentId = currentId;
        }

        protected async Task<bool> BeUniqueAsync<TValue>(
            TDto dto,
            TValue value,
            Func<TValue, Task<TEntity?>> getByValueAsync,
            CancellationToken cancellationToken)
        {
            if (value == null) return true;

            var existing = await getByValueAsync(value);
            return existing == null || (CurrentId.HasValue && existing.GetType().GetProperty("Id")?.GetValue(existing)?.Equals(CurrentId.Value) == true);
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