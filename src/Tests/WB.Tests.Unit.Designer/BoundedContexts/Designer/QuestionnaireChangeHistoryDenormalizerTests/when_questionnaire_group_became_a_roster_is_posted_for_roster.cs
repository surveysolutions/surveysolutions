using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    internal class when_questionnaire_group_became_a_roster_is_posted_for_roster : QuestionnaireChangeHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            var questionnaireStateTacker = Create.QuestionnaireStateTacker();
            questionnaireStateTacker.RosterState[rosterId] = rosterTitle;

            questionnaireStateTackerStorage =
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireStateTracker>>(
                    _ => _.GetById(Moq.It.IsAny<string>()) == questionnaireStateTacker);
            questionnaireChangeRecordStorage = new TestInMemoryWriter<QuestionnaireChangeRecord>();

            questionnaireChangeHistoryDenormalizer =
                CreateQuestionnaireChangeHistoryDenormalizer(questionnaireStateTacker: questionnaireStateTackerStorage,
                    questionnaireChangeRecord: questionnaireChangeRecordStorage);
        };

        Because of = () =>
            questionnaireChangeHistoryDenormalizer.Handle(Create.GroupBecameARosterEvent(rosterId.FormatGuid()));

        It should_store_0_changes = () =>
            GetAllRecords(questionnaireChangeRecordStorage).Length.ShouldEqual(0);

        private static QuestionnaireChangeHistoryDenormalizer questionnaireChangeHistoryDenormalizer;
        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");

        private static string rosterTitle = "test";
        private static IReadSideKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTackerStorage;
        private static TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
    }
}
