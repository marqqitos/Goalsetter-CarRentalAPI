using CarRental.DTOs;
using FluentValidation;
using ValidationException = FluentValidation.ValidationException;

namespace CarRental.Validators
{
    public class VehicleDTOValidator : AbstractValidator<VehicleDTO>
    {
        public VehicleDTOValidator()
        {
            RuleFor(x => x.ChassisNumber).NotEmpty().WithMessage("Chassis Number is required");
            RuleFor(x => x.Make).NotEmpty().WithMessage("Make is required");
            RuleFor(x => x.Model).NotEmpty().WithMessage("Model is required");
            RuleFor(x => x.PricePerDay).NotEmpty().WithMessage("Price is required")
                                       .GreaterThan(0).WithMessage("Price must be greater than 0");
        }
    }
}
