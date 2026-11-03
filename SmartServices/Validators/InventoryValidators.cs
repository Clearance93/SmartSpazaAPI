using FluentValidation;
using SmartDto.Inventory;

namespace SmartServices.Validators
{
    public class ProductDtoValidator : AbstractValidator<ProductDto>
    {
        public ProductDtoValidator()
        {
            RuleFor(x => x.Barcode).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Category).MaximumLength(100);
            RuleFor(x => x.Unit).MaximumLength(50);
            RuleFor(x => x.StockKeepUnit).MaximumLength(64);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        }
    }

    public class CheckoutItemDtoValidator : AbstractValidator<CheckoutItemDto>
    {
        public CheckoutItemDtoValidator()
        {
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x)
                .Must(item => item.ProductId != Guid.Empty || !string.IsNullOrWhiteSpace(item.Barcode))
                .WithMessage("A product id or barcode is required for every checkout item.");
        }
    }

    public class CheckoutDtoValidator : AbstractValidator<CheckoutDto>
    {
        public CheckoutDtoValidator()
        {
            RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required to complete a checkout.");
            RuleForEach(x => x.Items).SetValidator(new CheckoutItemDtoValidator());
        }
    }
}
