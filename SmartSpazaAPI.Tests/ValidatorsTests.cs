using SmartDto;
using SmartDto.Inventory;
using SmartServices.Validators;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class ValidatorsTests
    {
        [Fact]
        public void RegisterDto_WithUnsupportedRole_IsInvalid()
        {
            var validator = new RegisterDtoValidator();

            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "user@example.com",
                Password = "Passw0rd!",
                WhatsAppNumber = "0821234567",
                Role = "Administrator"
            };

            Assert.False(validator.Validate(dto).IsValid);
        }

        [Fact]
        public void RegisterDto_WithShortPassword_IsInvalid()
        {
            var validator = new RegisterDtoValidator();

            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "user@example.com",
                Password = "short",
                WhatsAppNumber = "0821234567",
                Role = "Driver"
            };

            Assert.False(validator.Validate(dto).IsValid);
        }

        [Fact]
        public void RegisterDto_WithValidValues_IsValid()
        {
            var validator = new RegisterDtoValidator();

            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "user@example.com",
                Password = "Passw0rd!",
                WhatsAppNumber = "0821234567",
                Role = "Driver"
            };

            Assert.True(validator.Validate(dto).IsValid);
        }

        [Fact]
        public void UserLoginDto_WithInvalidEmail_IsInvalid()
        {
            var validator = new UserLoginDtoValidator();

            var dto = new UserLoginDto { Email = "not-an-email", Password = "x" };

            Assert.False(validator.Validate(dto).IsValid);
        }

        [Fact]
        public void CheckoutDto_WithoutItems_IsInvalid()
        {
            var validator = new CheckoutDtoValidator();

            var dto = new CheckoutDto { Items = new List<CheckoutItemDto>() };

            Assert.False(validator.Validate(dto).IsValid);
        }

        [Fact]
        public void CheckoutDto_WithValidItem_IsValid()
        {
            var validator = new CheckoutDtoValidator();

            var dto = new CheckoutDto
            {
                Items = new List<CheckoutItemDto>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 2 }
                }
            };

            Assert.True(validator.Validate(dto).IsValid);
        }
    }
}
