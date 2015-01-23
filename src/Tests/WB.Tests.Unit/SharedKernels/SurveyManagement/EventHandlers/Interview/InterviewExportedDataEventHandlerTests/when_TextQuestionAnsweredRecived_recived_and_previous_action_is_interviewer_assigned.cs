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
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_TextQuestionAnsweredRecived_recived_and_previous_action_is_interviewer_assigned : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportService = new Mock<IDataExportRepositoryWriter>();
            recordFirstAnswerMarkerViewStorage = new Mock<IReadSideKeyValueStorage<RecordFirstAnswerMarkerView>>();
            foreach (var questionAnswered in ListOfQuestionAnsweredEventsHandledByDenormalizer)
            {
                var interviewId = Guid.NewGuid();

                eventsAndInterviewActionLog.Add(interviewId, questionAnswered);
                recordFirstAnswerMarkerViewStorage.Setup(x => x.GetById(Moq.It.IsAny<string>()))
                    .Returns(new RecordFirstAnswerMarkerView(interviewId));
            }
            
            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                dataExportService.Object, recordFirstAnswerMarkerViewStorage.Object, new UserDocument() { UserName = "user name", Roles = new List<UserRoles> { UserRoles.Operator } });
        };

        Because of = () => HandleQuestionAnsweredEventsByDenormalizer(interviewExportedDataDenormalizer, eventsAndInterviewActionLog);

        It should_FirstAnswerSet_action_be_added_to_dataExport = () =>
                 dataExportService.Verify(x => x.AddInterviewAction(
                        InterviewExportedAction.FirstAnswerSet,
                        Moq.It.IsAny<Guid>(), Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()));

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static Mock<IDataExportRepositoryWriter> dataExportService;
        private static Mock<IReadSideKeyValueStorage<RecordFirstAnswerMarkerView>> recordFirstAnswerMarkerViewStorage;
        private static Dictionary<Guid, QuestionAnswered> eventsAndInterviewActionLog = new Dictionary<Guid, QuestionAnswered>();
    }
}
