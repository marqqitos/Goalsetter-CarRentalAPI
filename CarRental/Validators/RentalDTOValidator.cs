using CarRental.DTOs;
using FluentValidation;

namespace CarRental.Validators
{
    public class RentalDTOValidator : AbstractValidator<RentalDTO>
    {
        public RentalDTOValidator()
        {
            RuleFor(rental => rental.Vehicle).NotNull().WithMessage("Vehicle is required").SetValidator(new VehicleDTOValidator());
            RuleFor(rental => rental.Client).NotNull().WithMessage("Client is required").SetValidator(new ClientDTOValidator());
            RuleFor(rental => rental.StartDate.Date).NotNull().WithMessage("Start date is required")
                                             .GreaterThan(DateTime.Today.AddDays(-1)).WithMessage("Start date can not be in the past");
            RuleFor(rental => rental.EndDate.Date).NotNull().WithMessage("End date is required.")
                                             .GreaterThan(rental => rental.StartDate).WithMessage("End date must be greater than start date.")
                                             .GreaterThan(DateTime.Today).WithMessage("End date must be greater than today");
        }
    }
}
