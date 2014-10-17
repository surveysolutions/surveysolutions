using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_InterviewerAssigned_recived : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportServiceMock = CreateDataExportService();
            questionnarie = CreateQuestionnaireDocument(new Dictionary<string, Guid>());

            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewData, dataExportServiceMock.Object, new UserDocument() { UserName = "user name", Roles = new List<UserRoles> { UserRoles.User } });
        };

        Because of = () =>
            interviewExportedDataDenormalizer.Handle(CreatePublishableEvent(() => new InterviewerAssigned(Guid.NewGuid(), Guid.NewGuid()),
                interviewId));

        It should_InterviewerAssigned_action_be_added_to_dataExport = () =>
           dataExportServiceMock.Verify(
               x =>
                   x.AddInterviewAction(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>(),
                       Moq.It.Is<InterviewActionExportView>(view => view.Action == InterviewExportedAction.InterviewerAssigned)));

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static QuestionnaireDocument questionnarie;
        private static Guid interviewId = Guid.NewGuid();
        private static Mock<IDataExportService> dataExportServiceMock;
    }
}
