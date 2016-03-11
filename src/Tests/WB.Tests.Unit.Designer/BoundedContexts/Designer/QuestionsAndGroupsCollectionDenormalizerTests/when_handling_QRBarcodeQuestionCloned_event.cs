using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionsAndGroupsCollectionDenormalizerTests
{
    internal class when_handling_QRBarcodeQuestionCloned_event : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();
            questionDetailsFactoryMock = new Mock<IQuestionDetailsViewMapper>();
            questionDetailsFactoryMock
                .Setup(x => x.Map(Moq.It.IsAny<IQuestion>(), Moq.It.IsAny<Guid>()))
                .Returns((IQuestion q, Guid p) => new NumericDetailsView
                {
                    Id = q.PublicKey,
                    ParentGroupId = p,
                    QuestionScope = q.QuestionScope
                });

            questionFactoryMock = new Mock<IQuestionnaireEntityFactory>();
            questionFactoryMock
                .Setup(x => x.CreateQuestion(Moq.It.IsAny<QuestionData>()))
                .Returns((QuestionData q) => new TextQuestion { PublicKey = q.PublicKey, QuestionScope = q.QuestionScope});

            evnt = CreateQRBarcodeQuestionClonedEvent(questionId, parentGroupId: g3Id, scope: scope);

            denormalizer = CreateQuestionnaireInfoDenormalizer(
                questionDetailsViewMapper: questionDetailsFactoryMock.Object,
                questionnaireEntityFactory: questionFactoryMock.Object);
        };

        Because of = () =>
            newState = denormalizer.Update(previousState, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_questions_collection_in_result_view = () =>
            newState.Questions.ShouldNotBeNull();

        It should_return_7_items_in_questions_collection = () =>
            newState.Questions.Count.ShouldEqual(7);

        It should_return_question_N7_with_parent_id_equals_g3Id = () =>
            newState.Questions.Single(x => x.Id == questionId).ParentGroupId.ShouldEqual(g3Id);

        It should_return_question_N7_with_parent_group_ids_contains_only_g3Id_g2Id_g1Id = () =>
            newState.Questions.Single(x => x.Id == questionId).ParentGroupsIds.ShouldContainOnly(g3Id, g2Id, g1Id);

        It should_return_question_N7_with_roster_scope_ids_contains_only_g3Id_q2Id = () =>
            newState.Questions.Single(x => x.Id == questionId).RosterScopeIds.ShouldContainOnly(g3Id, q2Id);

        It should_return_question_N7_with_provided_scope = () =>
            newState.Questions.Single(x => x.Id == questionId).QuestionScope.ShouldEqual(scope);

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<QRBarcodeQuestionCloned> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionDetailsViewMapper> questionDetailsFactoryMock = null;
        private static Mock<IQuestionnaireEntityFactory> questionFactoryMock;
        private static Guid questionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static QuestionScope scope = QuestionScope.Hidden;
    }
}