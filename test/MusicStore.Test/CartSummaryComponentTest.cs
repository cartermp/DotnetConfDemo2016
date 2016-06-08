using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicStore.Controllers;
using MusicStore.Models;
using Xunit;

namespace MusicStore.Components
{
    public class CartSummaryComponentTest
    {
        private readonly IServiceProvider _serviceProvider;

        public CartSummaryComponentTest()
        {
            var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

            var services = new ServiceCollection();

            services.AddDbContext<MusicStoreContext>(b => b.UseInMemoryDatabase().UseInternalServiceProvider(efServiceProvider));

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task CartSummaryComponent_Returns_CartedItems()
        {
            // Arrange
            var viewContext = new ViewContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Session initialization
            var cartId = "CartId_A";
            viewContext.HttpContext.Session = new TestSession();
            viewContext.HttpContext.Session.SetString("Session", cartId);

            // DbContext initialization
            var dbContext = _serviceProvider.GetRequiredService<MusicStoreContext>();
            PopulateData(dbContext, cartId, albumTitle: "AlbumA", itemCount: 10);

            // CartSummaryComponent initialization
            var cartSummaryComponent = new CartSummaryComponent(dbContext)
            {
                ViewComponentContext = new ViewComponentContext() { ViewContext = viewContext }
            };

            // Act
            var result = await cartSummaryComponent.InvokeAsync();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Null(viewResult.ViewData.Model);
            Assert.Equal(10, cartSummaryComponent.ViewBag.CartCount);
            Assert.Equal("AlbumA", cartSummaryComponent.ViewBag.CartSummary);
        }

        private static void PopulateData(MusicStoreContext context, string cartId, string albumTitle, int itemCount)
        {
            var album = new Album()
            {
                AlbumId = 1,
                Title = albumTitle,
            };

            var cartItems = Enumerable.Range(1, itemCount).Select(n =>
                new CartItem()
                {
                    AlbumId = 1,
                    Album = album,
                    Count = 1,
                    CartId = cartId,
                }).ToArray();

            context.AddRange(cartItems);
            context.SaveChanges();
        }
    }
}
