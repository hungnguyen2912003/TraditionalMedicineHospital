using FluentValidation;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Validators
{
    public class MedicineValidator : BaseUniqueValidator<MedicineDto, Medicine>
    {
        private readonly IMedicineRepository _repository;

        public MedicineValidator(IMedicineRepository repository,Guid? currentId = null) : base(currentId)
        {
            _repository = repository;

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Mã thuốc không được bỏ trống.")
                .Length(4, 10).WithMessage("Mã thuốc phải từ 4 đến 10 ký tự.")
                .Matches("^[A-Za-z][A-Za-z0-9]*$").WithMessage("Mã phải bắt đầu bằng chữ cái và chỉ chứa chữ cái hoặc số.")
                .MustAsync((dto, code, cancellation) => BeUniqueAsync(dto, code, _repository.GetByCodeAsync, cancellation))
                .WithMessage("Mã thuốc đã tồn tại.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên thuốc không được bỏ trống.")
                .Length(6, 20).WithMessage("Tên thuốc phải từ 6 đến 20 ký tự.")
                .Matches("^[A-Za-zÀ-ỹ][A-Za-zÀ-ỹ0-9 ]*$").WithMessage("Tên phải bắt đầu bằng chữ cái và chỉ chứa chữ cái, số hoặc khoảng trắng.");

            RuleFor(x => x.MedicineCategoryId)
                .NotEmpty().WithMessage("Vui lòng chọn loại thuốc.");

            RuleFor(x => x.Manufacturer)
                .NotEmpty().WithMessage("Nhà sản xuất không được bỏ trống.")
                .Length(2, 50).WithMessage("Nhà sản xuất phải từ 2 đến 50 ký tự.");


            RuleFor(x => x.ManufacturedDate)
                .NotEmpty().WithMessage("Ngày sản xuất không được bỏ trống.")
                .MustAsync(BeAValidDate).WithMessage("Vui lòng nhập ngày hợp lệ.");

            RuleFor(x => x.ExpiryDate)
                .NotEmpty().WithMessage("Ngày hết hạn không được bỏ trống.")
                .MustAsync(BeAValidDate).WithMessage("Vui lòng nhập ngày hợp lệ.")
                .GreaterThan(x => x.ManufacturedDate).WithMessage("Ngày hết hạn phải sau ngày sản xuất.");

            RuleFor(x => x.StockQuantity)
                .NotEmpty().WithMessage("Số lượng tồn kho không được bỏ trống.")
                .GreaterThan(0).WithMessage("Số lượng tồn kho phải lớn hơn 0.");

            RuleFor(x => x.StockUnit)
                .NotEmpty().WithMessage("Vui lòng chọn đơn vị.")
                .IsInEnum().WithMessage("Vui lòng chọn đơn vị hợp lệ.");

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("Giá bán không được bỏ trống.")
                .GreaterThan(0).WithMessage("Giá bán phải lớn hơn 0.");

            RuleFor(x => x.ActiveIngredient)
                .NotEmpty().WithMessage("Thành phần hoạt chất không được bỏ trống.")
                .Length(2, 100).WithMessage("Thành phần hoạt chất phải từ 2 đến 100 ký tự.");
        }
    }
}
