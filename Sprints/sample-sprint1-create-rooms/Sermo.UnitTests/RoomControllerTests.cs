using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Routing;

using Sermo.Contracts;
using Sermo.UI.Controllers;
using Sermo.UI.ViewModels;

using NUnit.Framework;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace Sermo.UnitTests {
    [TestFixture]
    public class RoomControllerTests {
        [SetUp]
        public void SetUp() {
            mockRoomRepository = new Mock<IRoomRepository>();
        }

        private RoomController CreateController() {
            return new RoomController(mockRoomRepository.Object);
        }

        private Mock<IRoomRepository> mockRoomRepository;

        [Test]
        public void ConstructingWithoutRepositoryThrowsArgumentNullException() {
            Assert.Throws<ArgumentNullException>(() => new RoomController(null));
        }

        [Test]
        public void ConstructingWithValidParametersDoesNotThrowException() {
            Assert.DoesNotThrow(() => CreateController());
        }

        [Test]
        public void GetCreateRendersView() {
            var controller = CreateController();
            var result = controller.Create();
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void GetCreateSetsViewModel() {
            var controller = CreateController();
            var result = controller.Create() as ViewResult;
            Assert.IsInstanceOf<CreateRoomViewModel>(result.Model);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void PostCreateNewRoomWithInvalidRoomNameCausesValidationError(string roomName) {
            var controller = CreateController();
            var viewModel = new CreateRoomViewModel { NewRoomName = roomName };

            var context = new ValidationContext(viewModel, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(viewModel, context, results);

            Assert.IsFalse(isValid);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void PostCreateNewRoomWithInvalidRoomNameShowsCreateView(string roomName) {
            var controller = CreateController();
            var viewModel = new CreateRoomViewModel { NewRoomName = roomName };

            controller.ModelState.AddModelError("Room name", "Room name is required.");
            var result = controller.Create(viewModel);

            Assert.IsInstanceOf<ViewResult>(result);

            var viewResult = result as ViewResult;
            Assert.AreEqual(viewResult.Model, viewModel);
            Assert.AreEqual(viewResult.View, null);
        }

        [Test]
        public void PostCreateNewRoomRedirectsToViewResult() {
            var controller = CreateController();
            var viewModel = new CreateRoomViewModel { NewRoomName = "new room" };

            var result = controller.Create(viewModel);
            Assert.IsInstanceOf<RedirectToRouteResult>(result);

            var redirestResult = result as RedirectToRouteResult;
            Assert.AreEqual(redirestResult.RouteValues["Action"], "List");
        }

        [Test]
        public void PostCreateNewRoomDelegatesToRoomRepository() {
            var controller = CreateController();
            var viewModel = new CreateRoomViewModel { NewRoomName = "new room" };

            controller.Create(viewModel);

            mockRoomRepository.Verify(m => m.CreateRoom("new room"), Times.Once);
        }
    }
}
