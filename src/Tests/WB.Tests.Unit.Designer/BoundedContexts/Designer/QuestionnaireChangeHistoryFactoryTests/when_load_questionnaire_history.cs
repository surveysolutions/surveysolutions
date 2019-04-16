using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;


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

            questionnaireChangeRecordStorage= Create.InMemoryDbContext();

            questionnaireChangeRecordStorage.Add(
                Create.QuestionnaireChangeRecord(
                    questionnaireId: questionnaireId.FormatGuid(),
                    targetId: questionId,
                    targetType: QuestionnaireItemType.Question,
                    action: QuestionnaireActionType.Clone,
                    reference: new[] {Create.QuestionnaireChangeReference()}));

            questionnaireChangeRecordStorage.Add(
                Create.QuestionnaireChangeRecord(
                    questionnaireId: questionnaireId.FormatGuid(),
                    targetType: QuestionnaireItemType.Question,
                    action: QuestionnaireActionType.Update,
                    targetId: questionId));
            questionnaireChangeRecordStorage.SaveChanges();

            questionnaireChangeHistoryFactory =
                CreateQuestionnaireChangeHistoryFactory(
                    questionnaireChangeRecordStorage,
                    questionnaireDocumentStorage:
                        Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(
                            _ => _.GetById(Moq.It.IsAny<string>()) == questionnaireDocument));
            
            BecauseOf();
        }

        private void BecauseOf() =>
            result = questionnaireChangeHistoryFactory.Load(questionnaireId, 1, 20);

        [NUnit.Framework.Test] public void should_return_2_hostorical_records () =>
            result.ChangeHistory.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_first_historical_record_be_clone () =>
            result.ChangeHistory[0].ActionType.Should().Be(QuestionnaireActionType.Clone);

        [NUnit.Framework.Test] public void should_first_historical_has_parent_id () =>
           result.ChangeHistory[0].TargetParentId.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_first_historical_has_one_reference () =>
            result.ChangeHistory[0].HistoricalRecordReferences.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_second_historical_record_be_clone () =>
           result.ChangeHistory[1].ActionType.Should().Be(QuestionnaireActionType.Update);

        private static QuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory;
        private static DesignerDbContext questionnaireChangeRecordStorage;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireChangeHistory result;
    }
}
