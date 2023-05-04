using CarRental.DTOs;
using FluentValidation;

namespace CarRental.Validators
{
    public class ClientDTOValidator : AbstractValidator<ClientDTO>
    {
        public ClientDTOValidator() 
        {
            RuleFor(client => client.FirstName).NotEmpty();
            RuleFor(client => client.LastName).NotEmpty();
            RuleFor(client => client.Email).NotEmpty().EmailAddress();
        }
    }
}
