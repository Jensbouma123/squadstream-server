// using NUnit.Framework;
// using Moq;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.Extensions.Configuration;
// using Web_API_new.Controllers;
// using Web_API_new.Models;
// using Web_API_new.DTOs;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using System.Collections.Generic;
// using Web_API_new.Data;
//
// [TestFixture]
// public class AuthControllerTests
// {
//     // private Mock<UserManager<UserModel>> _mockUserManager;
//     // private Mock<SignInManager<UserModel>> _mockSignInManager;
//     // private Mock<IConfiguration> _mockConfiguration;
//     // private Mock<DataContext> _mockContext;
//     //
//     // [SetUp]
//     // public void SetUp()
//     // {
//     //     var userStoreMock = new Mock<IUserStore<UserModel>>();
//     //     _mockUserManager = new Mock<UserManager<UserModel>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
//     //     _mockSignInManager = new Mock<SignInManager<UserModel>>(_mockUserManager.Object, new Mock<IHttpClientFactory>().Object, new Mock<IUserClaimsPrincipalFactory<UserModel>>().Object, null, null, null, null);
//     //     _mockConfiguration = new Mock<IConfiguration>();
//     //     _mockContext = new Mock<DataContext>();
//     // }
//     //
//     // [Test]
//     // public async Task Register_ReturnsBadRequest_WhenUserExists()
//     // {
//     //     // Arrange
//     //     var controller = new AuthController(_mockUserManager.Object, _mockSignInManager.Object, _mockConfiguration.Object, _mockContext.Object);
//     //     var registerDto = new RegisterDTO { Email = "test@test.com", Password = "Test1234" };
//     //     _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new UserModel());
//     //
//     //     // Act
//     //     var result = await controller.Register(registerDto);
//     //
//     //     // Assert
//     //     Assert.IsInstanceOf<BadRequestObjectResult>(result);
//     // }
//     //
//     // [Test]
//     // public async Task Login_ReturnsUnauthorized_WhenUserDoesNotExist()
//     // {
//     //     // Arrange
//     //     var controller = new AuthController(_mockUserManager.Object, _mockSignInManager.Object, _mockConfiguration.Object, _mockContext.Object);
//     //     var loginDto = new LoginDTO { Email = "test@test.com", Password = "Test1234" };
//     //     _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((UserModel)null);
//     //
//     //     // Act
//     //     var result = await controller.Login(loginDto);
//     //
//     //     // Assert
//     //     Assert.IsInstanceOf<UnauthorizedResult>(result);
//     // }
// }