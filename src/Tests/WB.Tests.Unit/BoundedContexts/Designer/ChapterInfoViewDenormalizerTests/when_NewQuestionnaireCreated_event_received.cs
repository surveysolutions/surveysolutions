using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Tests.Unit.BoundedContexts.Designer.ChapterInfoViewDenormalizerTests
{
    internal class when_NewQuestionnaireCreated_event_received : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            denormalizer = CreateDenormalizer();
        };

        Because of = () =>
            viewState = denormalizer.Update(null, Create.NewQuestionnaireCreatedEvent(questionnaireId));

        It should_groupInfoView_Id_be_equal_to_questionnaireId = () =>
            viewState.ItemId.ShouldEqual(questionnaireId);

        It should_groupInfoView_Items_not_be_null = () =>
            viewState.Items.ShouldNotBeNull();

        private static string questionnaireId = "33333333333333333333333333333333";
        
        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
