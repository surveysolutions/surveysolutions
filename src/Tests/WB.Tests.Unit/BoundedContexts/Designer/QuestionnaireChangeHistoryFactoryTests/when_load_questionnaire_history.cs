using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireChangeHistoryFactoryTests
{
    internal class when_load_questionnaire_history : QuestionnaireChangeHistoryFactoryTestContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new []
            {
                Create.Group(children: new[]
                {
                    Create.Question(questionId: questionId)
                })
            });

            questionnaireChangeRecordStorage=new TestInMemoryWriter<QuestionnaireChangeRecord>();

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
                        Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(
                            _ => _.GetById(Moq.It.IsAny<string>()) == questionnaireDocument));
        };

        Because of = () =>
            result = questionnaireChangeHistoryFactory.Load(questionnaireId, 1, 20);

        It should_return_2_hostorical_records = () =>
            result.ChangeHistory.Count.ShouldEqual(2);

        It should_first_historical_record_be_clone = () =>
            result.ChangeHistory[0].ActionType.ShouldEqual(QuestionnaireActionType.Clone);

        It should_first_historical_has_parent_id = () =>
           result.ChangeHistory[0].TargetParentId.ShouldNotBeNull();

        It should_first_historical_has_one_reference = () =>
            result.ChangeHistory[0].HistoricalRecordReferences.Count.ShouldEqual(1);

        It should_second_historical_record_be_clone = () =>
           result.ChangeHistory[1].ActionType.ShouldEqual(QuestionnaireActionType.Update);

        private static QuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory;
        private static TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireChangeHistory result;
    }
}
