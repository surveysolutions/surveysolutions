using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
            plainFileRepository = new Mock<IPlainInterviewFileStorage>();
            plainFileRepository.Setup(x => x.StoreInterviewBinaryData(iterviewId, fileName, Moq.It.IsAny<byte[]>()))
                .Throws<ArgumentException>();
            controller = CreateSyncControllerWithFile(viewFactory: userFactory, stream: new MemoryStream(), plainFileRepository: plainFileRepository.Object, fileName: fileName);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                controller.PostFile("login", "password", iterviewId)) as HttpException;

        It should_exception_be_not_null = () =>
            exception.ShouldNotBeNull();

        It should_exception_http_code_be_equal_to_500 = () =>
            exception.GetHttpCode().ShouldEqual(500);

        private static SyncController controller;
        private static HttpException exception;
        private static Mock<IPlainInterviewFileStorage> plainFileRepository;
        private static Guid iterviewId = Guid.NewGuid();
        private static string fileName = "file name";
    }
}
