using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Supervisor.SyncControllerTests
{
    internal class when_pushing_file_with_valid_file : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var userLight = new UserLight() { Name = "test" };
            var globalInfo = Mock.Of<IGlobalInfoProvider>(x => x.GetCurrentUser() == userLight);

            var user = new UserWebView();
            var userFactory = Mock.Of<IUserWebViewFactory>(x => x.Load(Moq.It.IsAny<UserWebViewInputModel>()) == user);
            plainFileRepository = new Mock<IPlainInterviewFileStorage>();
            
            controller = CreateSyncControllerWithFile(viewFactory: userFactory, stream: new MemoryStream(), plainFileRepository: plainFileRepository.Object, fileName: fileName, globalInfo: globalInfo);
            
        };

        Because of = () => controller.PostFile(new PostFileRequest() { InterviewId = interviewId, FileName = fileName, Data = Convert.ToBase64String(new byte[0])});
        
        It should_file_be_Saved_in_plain_file_storage = () =>
            plainFileRepository.Verify(x => x.StoreInterviewBinaryData(interviewId, fileName, Moq.It.IsAny<byte[]>()), Times.Once);

        private static InterviewerSyncController controller;

        private static Mock<IPlainInterviewFileStorage> plainFileRepository;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string fileName = "file.test";
    }
}
