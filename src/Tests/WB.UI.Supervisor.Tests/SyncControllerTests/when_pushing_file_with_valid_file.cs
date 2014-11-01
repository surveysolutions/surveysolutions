using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.User;
using Moq;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.SyncControllerTests
{
    internal class when_pushing_file_with_valid_file : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var userLight = new UserLight() { Name = "test" };
            var globalInfo = Mock.Of<IGlobalInfoProvider>(x => x.GetCurrentUser() == userLight);

            var user = new UserView();
            var userFactory = Mock.Of<IViewFactory<UserViewInputModel, UserView>>(x => x.Load(Moq.It.IsAny<UserViewInputModel>()) == user);
            plainFileRepository = new Mock<IPlainInterviewFileStorage>();
            controller = CreateSyncControllerWithFile(viewFactory: userFactory, stream: new MemoryStream(), plainFileRepository: plainFileRepository.Object, fileName: fileName, globalInfo: globalInfo);
            
        };

        Because of = () => controller.PostFile(interviewId);

        It should_file_be_Saved_in_plain_file_storage = () =>
            plainFileRepository.Verify(x => x.StoreInterviewBinaryData(interviewId, fileName, Moq.It.IsAny<byte[]>()), Times.Once);

        private static InterviewerSyncController controller;
        
        private static Mock<IPlainInterviewFileStorage> plainFileRepository;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string fileName = "file.test";
        private static FormDataCollection formdata;
    }
}
