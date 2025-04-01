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
                .Length(8).WithMessage("Mã nhân sự phải có đúng 8 ký tự.")
                .MustAsync(async (code, cancellation) => !await _repository.IsCodeExistsAsync(code))
                .WithMessage("Mã nhân sự đã tồn tại.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Tên nhân sự không được bỏ trống.")
                .Length(6, 20).WithMessage("Tên nhân sự phải từ 6 đến 20 ký tự.")
                .Matches("^[A-Za-zÀ-ỹ][A-Za-zÀ-ỹ0-9 ]*$").WithMessage("Tên phải bắt đầu bằng chữ cái và chỉ chứa chữ cái, số hoặc khoảng trắng.");

            RuleFor(x => x.EmployeeCategoryId)
                .NotEmpty().WithMessage("Loại nhân sự không được bỏ trống.");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Giới tính không được bỏ trống.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Ngày sinh không được bỏ trống.")
                .MustAsync(BeAValidDate).WithMessage("Ngày sinh không hợp lệ.")
                .MustAsync(async (dateOfBirth, cancellation) => await BeOver18YearsOld(dateOfBirth))
                .WithMessage("Nhân sự phải trên 18 tuổi.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được bỏ trống.")
                .EmailAddress().WithMessage("Email không hợp lệ.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Số điện thoại không được bỏ trống.")
                .Length(10, 15).WithMessage("Số điện thoại phải từ 10 đến 15 ký tự.")
                .Matches("^[0-9]*$").WithMessage("Số điện thoại không hợp lệ.");

            RuleFor(x => x.IdentityCard)
                .NotEmpty().WithMessage("Căn cước công dân không được bỏ trống.")
                .MinimumLength(9).WithMessage("Căn cước công dân phải có ít nhất 9 ký tự.")
                .MaximumLength(12).WithMessage("Căn cước công dân không được vượt quá 12 ký tự.")
                .Matches(@"^[0-9]*$").WithMessage("Căn cước công dân không hợp lệ.")
                .MustAsync(async (identityCard, cancellation) => !await _repository.IsIdentityCardExistsAsync(identityCard))
                .WithMessage("Căn cước công dân đã tồn tại.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Địa chỉ không được bỏ trống.");

            RuleFor(x => x.Degree)
                .NotEmpty().WithMessage("Bằng cấp không được bỏ trống.");

            RuleFor(x => x.ProfessionalQualification)
                .NotEmpty().WithMessage("Trình độ chuyên môn không được bỏ trống.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Ngày bắt đầu làm không được bỏ trống.")
                .MustAsync(BeAValidDate).WithMessage("Ngày bắt đầu làm không hợp lệ.");
        }
    }
}
