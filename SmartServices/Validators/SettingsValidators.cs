using FluentValidation;
using SmartDto;
using SmartDto.Business;
using SmartDto.Driver;

namespace SmartServices.Validators
{
    public class AvailabilityDtoValidator : AbstractValidator<AvailabilityDto>
    {
        public AvailabilityDtoValidator()
        {
            RuleFor(x => x.Uid).NotEmpty();
        }
    }

    public class DriversDtoValidator : AbstractValidator<DriversDto>
    {
        public DriversDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.PhoneNumbers).MaximumLength(20);
            RuleFor(x => x.WhatsAppNumber).MaximumLength(20);
            RuleFor(x => x.DeliveryZone).MaximumLength(100);
        }
    }

    public class VehicleDtoValidator : AbstractValidator<VehicleDto>
    {
        public VehicleDtoValidator()
        {
            RuleFor(x => x.DriverId).NotEmpty();
            RuleFor(x => x.VehicleName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.VehicleRegistration).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LicenseNumber).MaximumLength(50);
        }
    }

    public class NavigationDtoValidator : AbstractValidator<NavigationDto>
    {
        public NavigationDtoValidator()
        {
            RuleFor(x => x.DriverId).NotEmpty();
        }
    }

    public class SystemNotifiationDtoValidator : AbstractValidator<SystemNotifiationDto>
    {
        public SystemNotifiationDtoValidator()
        {
            RuleFor(x => x.Uid).NotNull();
        }
    }
}
