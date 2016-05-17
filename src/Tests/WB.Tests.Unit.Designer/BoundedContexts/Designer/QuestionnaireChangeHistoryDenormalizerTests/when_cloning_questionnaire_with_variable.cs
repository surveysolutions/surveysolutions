using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    internal class when_cloning_questionnaire_with_variable : QuestionnaireChangeHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            variableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            variableName = "var1";

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.Variable(id: variableId,
                    variableName: variableName));

            questionnaireStateTackerStorage =
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireStateTracker>>(
                    _ => _.GetById(Moq.It.IsAny<string>()) == Create.QuestionnaireStateTacker());

            questionnaireChangeRecordStorage = new TestInMemoryWriter<QuestionnaireChangeRecord>();
            denormalizer = CreateQuestionnaireChangeHistoryDenormalizer(
                questionnaireStateTacker: questionnaireStateTackerStorage,
                questionnaireChangeRecord: questionnaireChangeRecordStorage);

            denormalizer.Handle(Create.Event.QuestionnaireCloned(questionnaire).ToPublishedEvent());
        };

        Because of = () =>
        {
            var variableDeleted = Create.Event.VariableDeleted(variableId).ToPublishedEvent();
            denormalizer.Handle(variableDeleted);
        };

        It should_be_able_to_record_variable_related_changes_in_copy = () => GetAllRecords(questionnaireChangeRecordStorage).Length.ShouldEqual(2);

        static Guid variableId;
        static string variableName;
        static QuestionnaireChangeHistoryDenormalizer denormalizer;
        static TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
        static IReadSideKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTackerStorage;
    }
}