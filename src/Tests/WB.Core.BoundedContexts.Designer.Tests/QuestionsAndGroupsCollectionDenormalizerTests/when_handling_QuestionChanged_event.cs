using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionsAndGroupsCollectionDenormalizerTests
{
    internal class when_handling_QuestionChanged_event : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();
            questionDetailsFactoryMock = new Mock<IQuestionDetailsViewMapper>();
            questionDetailsFactoryMock
                .Setup(x => x.Map(Moq.It.IsAny<IQuestion>(), Moq.It.IsAny<Guid>()))
                .Returns((IQuestion q, Guid p) => new TextDetailsView
                {
                    Id = q.PublicKey,
                    ParentGroupId = p,
                    Title = newTitle
                });

            questionFactoryMock = new Mock<IQuestionFactory>();
            questionFactoryMock
                .Setup(x => x.CreateQuestion(Moq.It.IsAny<QuestionData>()))
                .Returns((QuestionData q) => new TextQuestion { PublicKey = q.PublicKey });
            
            evnt = CreateQuestionChangedEvent(q4Id, newTitle, QuestionType.Numeric);

            denormalizer = CreateQuestionnaireInfoDenormalizer(
                questionDetailsViewMapper: questionDetailsFactoryMock.Object,
                questionFactory: questionFactoryMock.Object);
        };

        Because of = () =>
            newState = denormalizer.Update(previousState, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_questions_collection_in_result_view = () =>
            newState.Questions.ShouldNotBeNull();

        It should_return_6_items_in_questions_collection = () =>
            newState.Questions.Count.ShouldEqual(6);

        It should_return_question_N7_with_title_equals_newTitle = () =>
            newState.Questions.Single(x => x.Id == q4Id).Title.ShouldEqual(newTitle);

        It should_return_question_N7_with_parent_id_equals_g3Id = () =>
            newState.Questions.Single(x => x.Id == q4Id).ParentGroupId.ShouldEqual(g3Id);

        It should_return_question_N7_with_parent_group_ids_contains_only_g3Id_g2Id_g1Id = () =>
            newState.Questions.Single(x => x.Id == q4Id).ParentGroupsIds.ShouldContainOnly(g3Id, g2Id, g1Id);

        It should_return_question_N7_with_roster_scope_ids_contains_only_g3Id_q2Id = () =>
            newState.Questions.Single(x => x.Id == q4Id).RosterScopeIds.ShouldContainOnly(g3Id, q2Id);

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<QuestionChanged> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionDetailsViewMapper> questionDetailsFactoryMock = null;
        private static Mock<IQuestionFactory> questionFactoryMock;
        private static string newTitle = "New title";
    }
}