using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionsAndGroupsCollectionDenormalizerTests
{
    internal class when_handling_NewQuestionnaireCreated_event : QuestionsAndGroupsCollectionDenormalizerTestContext
    {
        Establish context = () =>
        {
            evnt = CreateNewQuestionnaireCreatedEvent();
            denormalizer = CreateQuestionnaireInfoDenormalizer();
        };

        Because of = () =>
            newState = denormalizer.Update(null, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_questions_collection_in_result_view = () =>
            newState.Questions.ShouldBeEmpty();

        It should_return_not_null_groups_collection_in_result_sview = () =>
            newState.Groups.ShouldBeEmpty();

        It should_return_not_null_staitc_text_collection_in_result_view = () =>
           newState.StaticTexts.ShouldBeEmpty();

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<NewQuestionnaireCreated> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
    }
}