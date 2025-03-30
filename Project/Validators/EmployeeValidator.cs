using FluentValidation;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Validators
{
    public class EmployeeValidator : BaseUniqueValidator<EmployeeDto, Employee>
    {
        private readonly IEmployeeRepository _repository;

        public EmployeeValidator(IEmployeeRepository repository, Guid? currentId = null) : base(currentId)
        {
            _repository = repository;

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Mã nhân sự không được bỏ trống.")
                .Length(4, 10).WithMessage("Mã nhân sự phải từ 4 đến 10 ký tự.")
                .Matches("^[A-Za-z][A-Za-z0-9]*$").WithMessage("Mã phải bắt đầu bằng chữ cái và chỉ chứa chữ cái hoặc số.")
                .MustAsync((dto, code, cancellation) => BeUniqueAsync(dto, code, _repository.GetByCodeAsync, cancellation))
                .WithMessage("Mã nhân sự đã tồn tại.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Tên nhân sự không được bỏ trống.")
                .Length(6, 20).WithMessage("Tên nhân sự phải từ 6 đến 20 ký tự.")
                .Matches("^[A-Za-zÀ-ỹ][A-Za-zÀ-ỹ0-9 ]*$").WithMessage("Tên phải bắt đầu bằng chữ cái và chỉ chứa chữ cái, số hoặc khoảng trắng.");


        }
    }
}
