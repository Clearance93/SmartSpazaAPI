using SmartDto.Common;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class PaginationParamsTests
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(-10, 1)]
        [InlineData(1, 1)]
        [InlineData(7, 7)]
        public void Page_IsClampedToAtLeastOne(int input, int expected)
        {
            var pagination = new PaginationParams { Page = input };

            Assert.Equal(expected, pagination.Page);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-5, 1)]
        [InlineData(500, 100)]
        [InlineData(25, 25)]
        public void PageSize_IsClamped(int input, int expected)
        {
            var pagination = new PaginationParams { PageSize = input };

            Assert.Equal(expected, pagination.PageSize);
        }
    }
}
