using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryFactoryTests
{
    internal class when_load_questionnaire_history : QuestionnaireChangeHistoryFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new []
            {
                Create.Group(children: new[]
                {
                    Create.Question(questionId: questionId)
                })
            });

            questionnaireChangeRecordStorage=new InMemoryPlainStorageAccessor<QuestionnaireChangeRecord>();

            questionnaireChangeRecordStorage.Store(
                Create.QuestionnaireChangeRecord(questionnaireId: questionnaireId.FormatGuid(),
                    targetId: questionId,
                    targetType: QuestionnaireItemType.Question,
                    action: QuestionnaireActionType.Clone,
                    reference: new[] {Create.QuestionnaireChangeReference()}), "a");

            questionnaireChangeRecordStorage.Store(
                Create.QuestionnaireChangeRecord(
                    questionnaireId: questionnaireId.FormatGuid(),
                    targetType: QuestionnaireItemType.Question,
                    action: QuestionnaireActionType.Update,
                    targetId: questionId),
                "b");
            
            questionnaireChangeHistoryFactory =
                CreateQuestionnaireChangeHistoryFactory(
                    questionnaireChangeHistoryStorage: questionnaireChangeRecordStorage,
                    questionnaireDocumentStorage:
                        Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(
                            _ => _.GetById(Moq.It.IsAny<string>()) == questionnaireDocument));
            BecauseOf();
        }

        private void BecauseOf() =>
            result = questionnaireChangeHistoryFactory.Load(questionnaireId, 1, 20);

        [NUnit.Framework.Test] public void should_return_2_hostorical_records () =>
            result.ChangeHistory.Count.ShouldEqual(2);

        [NUnit.Framework.Test] public void should_first_historical_record_be_clone () =>
            result.ChangeHistory[0].ActionType.ShouldEqual(QuestionnaireActionType.Clone);

        [NUnit.Framework.Test] public void should_first_historical_has_parent_id () =>
           result.ChangeHistory[0].TargetParentId.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_first_historical_has_one_reference () =>
            result.ChangeHistory[0].HistoricalRecordReferences.Count.ShouldEqual(1);

        [NUnit.Framework.Test] public void should_second_historical_record_be_clone () =>
           result.ChangeHistory[1].ActionType.ShouldEqual(QuestionnaireActionType.Update);

        private static QuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory;
        private static InMemoryPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireChangeHistory result;
    }
}
