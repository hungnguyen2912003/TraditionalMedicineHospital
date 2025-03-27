using FluentValidation;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Validators
{
    public class MedicineCategoryValidator : BaseUniqueValidator<MedicineCategoryDto, MedicineCategory>
    {
        private readonly IMedicineCategoryRepository _repository;

        public MedicineCategoryValidator(IMedicineCategoryRepository repository, Guid? currentId = null)
            : base(currentId)
        {
            _repository = repository;

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Mã loại thuốc không được bỏ trống.")
                .Length(4, 10).WithMessage("Mã phải từ 4 đến 10 ký tự.")
                .Matches("^[A-Za-z][A-Za-z0-9]*$").WithMessage("Mã phải bắt đầu bằng chữ cái và chỉ chứa chữ cái hoặc số.")
                .MustAsync((dto, code, ct) => BeUniqueAsync(dto, code, _repository.GetByCodeAsync, ct))
                .WithMessage("Mã loại thuốc này đã tồn tại.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên loại thuốc không được bỏ trống.")
                .Length(6, 50).WithMessage("Tên phải từ 6 đến 50 ký tự.")
                .Matches("^[A-Za-zÀ-ỹ][A-Za-zÀ-ỹ0-9 ]*$").WithMessage("Tên phải bắt đầu bằng chữ cái và chỉ chứa chữ cái, số hoặc khoảng trắng.")
                .MustAsync((dto, name, ct) => BeUniqueAsync(dto, name, _repository.GetByNameAsync, ct))
                .WithMessage("Tên loại thuốc này đã tồn tại.");
        }
    }
}