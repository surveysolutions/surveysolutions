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
    internal class when_pushing_file_which_rise_exception_during_saving : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var user = new UserView();
            var userFactory = Mock.Of<IViewFactory<UserViewInputModel, UserView>>(x => x.Load(Moq.It.IsAny<UserViewInputModel>()) == user);
            plainFileRepository = new Mock<IPlainFileRepository>();
            plainFileRepository.Setup(x => x.StoreInterviewBinaryData(iterviewId, fileName, Moq.It.IsAny<byte[]>()))
                .Throws<ArgumentException>();
            controller = CreateSyncControllerWithFile(viewFactory: userFactory, stream: new MemoryStream(), plainFileRepository: plainFileRepository.Object, fileName: fileName);
        };

        Because of = () =>
            result = (JsonResult)controller.PostFile("login", "password", iterviewId);

        It should_return_false_result = () =>
            ((bool)result.Data).ShouldEqual(false);

        private static SyncController controller;
        private static JsonResult result;
        private static Mock<IPlainFileRepository> plainFileRepository;
        private static Guid iterviewId = Guid.NewGuid();
        private static string fileName = "file name";
    }
}
