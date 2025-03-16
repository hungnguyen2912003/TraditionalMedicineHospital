using FluentValidation;
using Project.Areas.Admin.Models.DTOs;

namespace Project.Validators
{
    public class MedicineCategoryValidator : AbstractValidator<MedicineCategoryDto>
    {
        public MedicineCategoryValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Mã loại thuốc không được bỏ trống")
                .MaximumLength(10).WithMessage("Mã loại thuốc không được vượt quá 10 ký tự");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên loại thuốc không được bỏ trống.")
                .MaximumLength(50).WithMessage("Tên loại thuốc không được vượt quá 50 ký tự.");
        }
    }
}
