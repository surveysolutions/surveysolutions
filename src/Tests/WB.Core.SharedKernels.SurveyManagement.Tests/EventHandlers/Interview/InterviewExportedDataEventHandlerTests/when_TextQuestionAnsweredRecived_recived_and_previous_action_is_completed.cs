using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_TextQuestionAnsweredRecived_recived_and_previous_action_is_completed : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportService = new Mock<IDataExportService>();
            questionnarie = CreateQuestionnaireDocument(new Dictionary<string, Guid>());

            foreach (var questionAnswered in ListOfQuestionAnsweredEventsHandledByDenormalizer)
            {
                var interviewId = Guid.NewGuid();

                eventsAndInterviewActionLog.Add(interviewId, questionAnswered);
            }

            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewData, dataExportService.Object, new UserDocument() { UserName = "user name", Roles = new List<UserRoles> { UserRoles.Operator } });
        };

        Because of = () => HandleQuestionAnsweredEventsByDenormalizer(interviewExportedDataDenormalizer, eventsAndInterviewActionLog);

        It should_interview_action_never_be_added = () => dataExportService.Verify(x=>x.AddInterviewAction(
            Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>(), Moq.It.IsAny<InterviewActionExportView>()),Times.Never);
        
        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static QuestionnaireDocument questionnarie;
        private static Mock<IDataExportService> dataExportService;
        private static Dictionary<Guid, QuestionAnswered> eventsAndInterviewActionLog = new Dictionary<Guid, QuestionAnswered>();
 
    }
}
