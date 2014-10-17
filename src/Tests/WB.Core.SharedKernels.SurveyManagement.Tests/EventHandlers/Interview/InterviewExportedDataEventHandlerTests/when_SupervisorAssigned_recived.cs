using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
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
    internal class when_SupervisorAssigned_recived : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportService = new Mock<IDataExportService>();
            questionnarie = CreateQuestionnaireDocument(new Dictionary<string, Guid>());

            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewData, dataExportService.Object, new UserDocument() { UserName = "user name" });
        };

        Because of = () =>
            interviewExportedDataDenormalizer.Handle(CreatePublishableEvent(() => new SupervisorAssigned(Guid.NewGuid(), Guid.NewGuid()),
                interviewId));

        It should_SupervisorAssigned_action_be_added_to_dataExport = () =>
            dataExportService.Verify(
           x =>
               x.AddInterviewAction(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>(),
                   Moq.It.Is<InterviewActionExportView>(view => view.Action == InterviewExportedAction.SupervisorAssigned)));


        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static QuestionnaireDocument questionnarie;
        private static Mock<IDataExportService> dataExportService;
        private static Guid interviewId = Guid.NewGuid();
    }
}
