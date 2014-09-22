using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.View;
using Main.Core.View.User;
using Moq;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.SyncControllerTests
{
    internal class when_pushing_file_with_valid_file : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var user = new UserView();
            var userFactory = Mock.Of<IViewFactory<UserViewInputModel, UserView>>(x => x.Load(Moq.It.IsAny<UserViewInputModel>()) == user);
            plainFileRepository = new Mock<IPlainInterviewFileStorage>();
            controller = CreateSyncControllerWithFile(viewFactory: userFactory, stream: new MemoryStream(), plainFileRepository: plainFileRepository.Object, fileName: fileName);
        };

        Because of = () =>
            result = (JsonResult)controller.PostFile("login", "password", iterviewId);

        It should_return_true_result = () =>
            ((bool)result.Data).ShouldEqual(true);

        It should_file_be_Saved_in_plain_file_storage = () =>
            plainFileRepository.Verify(x => x.StoreInterviewBinaryData(iterviewId, fileName, Moq.It.IsAny<byte[]>()), Times.Once);

        private static SyncController controller;
        private static JsonResult result;
        private static Mock<IPlainInterviewFileStorage> plainFileRepository;
        private static Guid iterviewId = Guid.NewGuid();
        private static string fileName = "file name";
    }
}
