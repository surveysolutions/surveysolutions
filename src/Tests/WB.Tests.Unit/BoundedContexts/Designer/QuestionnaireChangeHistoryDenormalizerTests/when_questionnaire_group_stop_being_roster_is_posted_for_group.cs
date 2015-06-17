using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    internal class when_questionnaire_group_stop_being_roster_is_posted_for_group : QuestionnaireChangeHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            var questionnaireStateTacker = Create.QuestionnaireStateTacker();
            questionnaireStateTacker.GroupsState[groupId] = groupTitle;

            questionnaireStateTackerStorage =
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireStateTacker>>(
                    _ => _.GetById(Moq.It.IsAny<string>()) == questionnaireStateTacker);
            questionnaireChangeRecordStorage = new TestInMemoryWriter<QuestionnaireChangeRecord>();

            questionnaireChangeHistoryDenormalizer =
                CreateQuestionnaireChangeHistoryDenormalizer(questionnaireStateTacker: questionnaireStateTackerStorage,
                    questionnaireChangeRecord: questionnaireChangeRecordStorage);
        };

        Because of = () =>
            questionnaireChangeHistoryDenormalizer.Handle(Create.GroupStoppedBeingARosterEvent(groupId.FormatGuid()));

        It should_store_0_changes = () =>
            GetAllRecords(questionnaireChangeRecordStorage).Length.ShouldEqual(0);

        private static QuestionnaireChangeHistoryDenormalizer questionnaireChangeHistoryDenormalizer;
        private static Guid groupId = Guid.Parse("11111111111111111111111111111111");

        private static string groupTitle = "test";
        private static IReadSideKeyValueStorage<QuestionnaireStateTacker> questionnaireStateTackerStorage;
        private static TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
    }
}
