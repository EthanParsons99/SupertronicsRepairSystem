using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SupertronicsRepairSystem.Controllers;
using SupertronicsRepairSystem.Services;
using SupertronicsRepairSystem.ViewModels;
using System.Threading.Tasks;
using System;

namespace SupertronicsRepairSystem.Tests
{
    [TestClass]
    public class OwnerControllerTests
    {
        // Mocks for all the services the OwnerController depends on
        private Mock<IAuthService> _mockAuthService;
        private Mock<IProductService> _mockProductService;
        private Mock<IQuoteService> _mockQuoteService;
        private Mock<IRepairJobService> _mockRepairJobService;

        // The controller we are testing
        private OwnerController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            // Create fresh mocks for each test
            _mockAuthService = new Mock<IAuthService>();
            _mockProductService = new Mock<IProductService>();
            _mockQuoteService = new Mock<IQuoteService>();
            _mockRepairJobService = new Mock<IRepairJobService>();

            // Instantiate the OwnerController with all its mocked dependencies
            _controller = new OwnerController(
                _mockAuthService.Object,
                _mockProductService.Object,
                _mockQuoteService.Object,
                _mockRepairJobService.Object
            );

            // Mock ASP.NET Core Framework Services to prevent crashes
            var httpContext = new DefaultHttpContext();
            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IAuthService))).Returns(_mockAuthService.Object);
            httpContext.RequestServices = services.Object;

            var tempDataFactory = new TempDataDictionaryFactory(new Mock<ITempDataProvider>().Object);
            _controller.TempData = tempDataFactory.GetTempData(httpContext);

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("a/fake/url");
            _controller.Url = mockUrlHelper.Object;

            _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
        }

        [TestMethod]
        public async Task AddProduct_WhenModelStateIsValid_CallsProductServiceAndRedirects()
        {
            // ARRANGE
            // Create the input data
            var viewModel = new AddProductViewModel
            {
                Name = "Test Product",
                Description = "A product for testing",
                ImageUrl = "http://test.com/image.png",
                Price = 99.99,
                WasPrice = 120.00,
                DiscountPercentage = 17,
                StockQuantity = 50,
                SerialNumber = "TESTING-SN-1234"
            };

            // Set up the mock service. When AddProductAsync is called with our ViewModel
            _mockProductService
                .Setup(service => service.AddProductAsync(viewModel))
                .ReturnsAsync(true);

            // ACT
            // Execute the controller action we want to test.
            var result = await _controller.AddProduct(viewModel);

            // ASSERT
            // Verify that the AddProductAsync method on our MOCK service was called exactly once.
            _mockProductService.Verify(service => service.AddProductAsync(viewModel), Times.Once);

            // Verify that the result is a RedirectToAction, sending the user to the right page.
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("ProductManagement", redirectResult.ActionName);

            // Verify that a success message was set in TempData
            Assert.IsNotNull(_controller.TempData["Success"]);
        }

        [TestMethod]
        public async Task AddTechnician_WhenModelStateIsValid_CallsAuthServiceAndRedirects()
        {
            // ARRANGE
            // Create the input data from the form.
            var viewModel = new AddTechnicianViewModel
            {
                Email = "test.tech@supertronics.com",
                Password = "a-strong-password",
                FirstName = "Test",
                Surname = "Tech",
                PhoneNumber = "0821234567"
            };

            // Set up the mock auth service. When SignUpAsync is called with the technician's details
            _mockAuthService
                .Setup(service => service.SignUpAsync(
                    viewModel.Email,
                    viewModel.Password,
                    viewModel.FirstName,
                    viewModel.Surname,
                    viewModel.PhoneNumber,
                    UserRole.Technician))
                .ReturnsAsync(new AuthResult { Success = true });

            // ACT
            // Execute the controller action.
            var result = await _controller.AddTechnician(viewModel);

            // ASSERT
            // Verify that the SignUpAsync method on our MOCK service was called exactly once with the correct parameters.
            _mockAuthService.Verify(service => service.SignUpAsync(
                viewModel.Email,
                viewModel.Password,
                viewModel.FirstName,
                viewModel.Surname,
                viewModel.PhoneNumber,
                UserRole.Technician),
            Times.Once);

            // Verify that the result is a RedirectToAction to the Dashboard.
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("Dashboard", redirectResult.ActionName);

            // Verify that a success message was set in TempData.
            Assert.IsNotNull(_controller.TempData["Success"]);
        }
    }
}