using System.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using SmartConfiguration;
using SmartCore.UnitOfWork;
using SmartDto.Inventory;
using SmartInterfaces.IRepository.IInventory;
using SmartModels;
using SmartServices;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class CheckoutTests
    {
        [Fact]
        public async Task Checkout_WhenStockIsInsufficient_ThrowsBusinessRuleException()
        {
            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Bread",
                StockQuantity = 1,
                IsDeleted = false
            };

            var productRepository = new Mock<IProductRepository>();
            productRepository.Setup(repository => repository.GetByIdAsync(product.ProductId))
                             .ReturnsAsync(product);

            var transaction = new Mock<IDbContextTransaction>();

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(work => work.Product).Returns(productRepository.Object);
            unitOfWork.Setup(work => work.BeginTransactionAsync(It.IsAny<IsolationLevel>()))
                      .ReturnsAsync(transaction.Object);

            var service = new ProductServices(unitOfWork.Object, Mock.Of<IMapper>());

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => service.CheckoutAsync(new CheckoutDto
            {
                Items = new List<CheckoutItemDto>
                {
                    new() { ProductId = product.ProductId, Quantity = 2 }
                }
            }));

            Assert.Contains("remain in stock", exception.Message);
            Assert.Equal(1, product.StockQuantity);
        }
    }
}
