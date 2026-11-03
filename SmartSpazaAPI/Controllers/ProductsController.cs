using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartConfiguration;
using SmartDto.Common;
using SmartDto.Inventory;
using SmartEnums;
using SmartInterfaces.IService;
using SmartSpazaAPI.Extensions;

namespace SmartSpazaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductServices _ProductServices;

        public ProductsController(IProductServices productServices)
        {
            _ProductServices = productServices ?? throw new ArgumentNullException(nameof(productServices));
        }

        [HttpGet("all-products")]
        public async Task<IActionResult> GetAllProducts([FromQuery] PaginationParams pagination, Guid id)
        {
            var products = await _ProductServices.GetPagedProductsAsync(pagination, id);

            return Ok(products);
        }

        [HttpGet("barcode")]
        public async Task<IActionResult> GetProductByBarcode(string code)
        {
            var product = await _ProductServices.GetProductByBarcodeAsync(code);

            return Ok(product);
        }

        [HttpGet("productById")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _ProductServices.GetProductByIdAsync(id);

            return Ok(product);
        }

        [HttpPost("add-product")]
        [Authorize(Policy = "BusinessUsers")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto dto)
        {
            var userId = User.RequireRoleUserId();

            if (User.IsInRole(RoleNames.Supplier))
            {
                dto.SupplierId = userId;
                dto.ShopOwnerId = null;
            }
            else
            {
                dto.ShopOwnerId = userId;
                dto.SupplierId = null;
            }

            var product = await _ProductServices.CreateProductAsync(dto);

            return Ok(product);
        }

        [HttpPut("update-product")]
        [Authorize(Policy = "BusinessUsers")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductDto dto)
        {
            var existing = await _ProductServices.GetProductByIdAsync(id);

            EnsureCanManage(existing);

            var product = await _ProductServices.UpdateProductAsync(id, dto);

            return Ok(product);
        }

        [HttpDelete("delete-product")]
        [Authorize(Policy = "BusinessUsers")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var existing = await _ProductServices.GetProductByIdAsync(id);

            EnsureCanManage(existing);

            await _ProductServices.DeleteProductAsync(id);

            return Ok(new
            {
                message = "The product was deleted successfully"
            });
        }

        [HttpPost("checkout")]
        [Authorize(Policy = "BusinessUsers")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            dto.ShopOwnerId = User.RequireRoleUserId();

            var updatedProducts = await _ProductServices.CheckoutAsync(dto);

            return Ok(new
            {
                message = "Checkout completed successfully",
                products = updatedProducts
            });
        }

        private void EnsureCanManage(ProductDto product)
        {
            var userId = User.RequireRoleUserId();

            if (User.IsInRole(RoleNames.Supplier))
            {
                if (product.SupplierId.HasValue && product.SupplierId.Value != userId)
                {
                    throw new ForbiddenException("You are not allowed to manage another supplier's product.");
                }

                if (product.ShopOwnerId.HasValue)
                {
                    throw new ForbiddenException("You are not allowed to manage a shop-owned product.");
                }

                return;
            }

            if (User.IsInRole(RoleNames.SpazaOwner))
            {
                if (product.ShopOwnerId != userId)
                {
                    throw new ForbiddenException("You are not allowed to manage another shop's product.");
                }

                return;
            }

            throw new ForbiddenException("You are not allowed to manage products.");
        }
    }
}
