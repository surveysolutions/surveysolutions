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
    internal class when_RejectedByHeadquarter_recived : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportService = new Mock<IDataExportService>();
            questionnarie = CreateQuestionnaireDocument(new Dictionary<string, Guid>());

            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewData, dataExportService.Object, new UserDocument() { UserName = "user name", Roles = new List<UserRoles>{UserRoles.Headquarter}});
        };

        Because of = () =>
            interviewExportedDataDenormalizer.Handle(CreatePublishableEvent(() => new InterviewRejectedByHQ(Guid.NewGuid(), "comment"),
                interviewId));

        It should_RejectedByHeadquarter_action_be_added_to_dataExport = () =>
         dataExportService.Verify(
             x =>
                 x.AddInterviewAction(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>(),
                     Moq.It.Is<InterviewActionExportView>(view => view.Action == InterviewExportedAction.RejectedByHeadquarter)));

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static QuestionnaireDocument questionnarie;
        private static Mock<IDataExportService> dataExportService;
        private static Guid interviewId = Guid.NewGuid();
    }
}
