using FluentValidation;
using SmartDto.Delivery;

namespace SmartServices.Validators
{
    public class DeliveryDtoValidator : AbstractValidator<DeliveryDto>
    {
        public DeliveryDtoValidator()
        {
            RuleFor(x => x.DriverId).NotEmpty();
            RuleFor(x => x.ShopId).NotEmpty();
            RuleFor(x => x.DeliveryNumber).NotEmpty().MaximumLength(20);
            RuleFor(x => x.DeliveryStatus).IsInEnum().When(x => x.DeliveryStatus != default);
            RuleFor(x => x.Priority).MaximumLength(50);
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }

    public class DeliveryTrackingDtoValidator : AbstractValidator<DeliveryTrackingDto>
    {
        public DeliveryTrackingDtoValidator()
        {
            RuleFor(x => x.DriverId).NotEmpty();
            RuleFor(x => x.DeliveryStatus).IsInEnum().When(x => x.DeliveryStatus != default);
            RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
            RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
            RuleFor(x => x.DistanceRemaining).GreaterThanOrEqualTo(0).When(x => x.DistanceRemaining.HasValue);
        }
    }

    public class DeliveryItemsDtoValidator : AbstractValidator<DeliveryItemsDto>
    {
        public DeliveryItemsDtoValidator()
        {
            RuleFor(x => x.DeliveryId).NotEmpty();
            RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.Unit).MaximumLength(50);
            RuleFor(x => x.Weight).MaximumLength(50);
            RuleFor(x => x.StockKeepUnit).MaximumLength(64);
        }
    }

    public class DeliveryRatingsDtoValidator : AbstractValidator<DeliveryRatingsDto>
    {
        public DeliveryRatingsDtoValidator()
        {
            RuleFor(x => x.DeliveryId).NotEmpty();
            RuleFor(x => x.DriverId).NotEmpty();
            RuleFor(x => x.Ratings).InclusiveBetween(0, 5).When(x => x.Ratings.HasValue);
            RuleFor(x => x.Comments).MaximumLength(1000);
        }
    }
}
