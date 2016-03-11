using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.ChapterInfoViewDenormalizerTests
{
    internal class when_TemplateImported_event_received : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            var expressionProcessor = new Mock<IExpressionProcessor>();
            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(chapter2QuestionConditionExpression))
                .Returns(new[] {variableUsedInChapter2Question});

            denormalizer = CreateDenormalizer(expressionProcessor: expressionProcessor.Object);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(null, Create.TemplateImportedEvent(questionnaireId: questionnaireId,
                    chapter1Id: chapter1Id, chapter1Title: chapter1Title, chapter1GroupId: chapter1GroupId,
                    chapter1GroupTitle: chapter1GroupTitle, chapter2Id: chapter2Id, chapter2Title: chapter2Title,
                    chapter2QuestionId: chapter2QuestionId, chapter2QuestionTitle: chapter2QuestionTitle,
                    chapter2QuestionVariable: chapter2QuestionVariable,
                    chapter2QuestionConditionExpression: chapter2QuestionConditionExpression,
                    chapter1StaticTextId: chapter1StaticTextId, chapter1StaticText: chapter1StaticText));

        It should_groupInfoView_Id_be_equal_to_questionnaireId = () =>
            viewState.ItemId.ShouldEqual(questionnaireId);

        It should_groupInfoView_Items_not_be_null = () =>
            viewState.Items.ShouldNotBeNull();

        It should_groupInfoView_Items_have_2_chapters = () =>
            viewState.Items.Count.ShouldEqual(2);

        It should_groupInfoView_first_chapter_id_be_equal_chapter1Id = () =>
            viewState.Items[0].ItemId.ShouldEqual(chapter1Id);

        It should_groupInfoView_first_chapter_title_be_type_of_groupInfoView = () =>
            viewState.Items[0].ShouldBeOfExactType<GroupInfoView>();

        It should_groupInfoView_first_chapter_title_be_equal_chapter1Title = () =>
            ((GroupInfoView)viewState.Items[0]).Title.ShouldEqual(chapter1Title);

        It should_groupInfoView_first_chapter_items_not_be_null = () =>
            ((GroupInfoView)viewState.Items[0]).Items.ShouldNotBeNull();

        It should_groupInfoView_first_chapter_contains_group_and_static_text = () =>
            ((GroupInfoView)viewState.Items[0]).Items.Count.ShouldEqual(2);

        It should_import_static_text = () =>
            ((GroupInfoView)viewState.Items[0]).Items[0].ShouldBeOfExactType<StaticTextInfoView>();

        It should_import_static_text_id = () =>
            ((StaticTextInfoView)((GroupInfoView)viewState.Items[0]).Items[0]).ItemId.ShouldEqual(chapter1StaticTextId);

        It should_import_static_text_text = () =>
            ((StaticTextInfoView)((GroupInfoView)viewState.Items[0]).Items[0]).Text.ShouldEqual(chapter1StaticText);

        It should_import_groups = () =>
            ((GroupInfoView)viewState.Items[0]).Items[1].ShouldBeOfExactType<GroupInfoView>();

        It should_import_group_id = () =>
            ((GroupInfoView)((GroupInfoView)viewState.Items[0]).Items[1]).ItemId.ShouldEqual(chapter1GroupId);

        It should_import_group_title = () =>
            ((GroupInfoView)((GroupInfoView)viewState.Items[0]).Items[1]).Title.ShouldEqual(chapter1GroupTitle);

        It should_groupInfoView_second_chapter_id_be_equal_chapter2Id = () =>
            viewState.Items[1].ItemId.ShouldEqual(chapter2Id);

        It should_groupInfoView_second_chapter_title_be_type_of_groupInfoView = () =>
            viewState.Items[1].ShouldBeOfExactType<GroupInfoView>();

        It should_groupInfoView_second_chapter_title_be_equal_chapter2Title = () =>
            ((GroupInfoView)viewState.Items[1]).Title.ShouldEqual(chapter2Title);

        It should_groupInfoView_second_chapter_items_not_be_null = () =>
            ((GroupInfoView)viewState.Items[1]).Items.ShouldNotBeNull();

        It should_groupInfoView_second_chapter_contains_1_item = () =>
            ((GroupInfoView)viewState.Items[1]).Items.Count.ShouldEqual(1);

        It should_groupInfoView_second_chapter_first_item_be_type_of_QuestionInfoView = () =>
            ((GroupInfoView)viewState.Items[1]).Items[0].ShouldBeOfExactType<QuestionInfoView>();

        It should_groupInfoView_second_chapter_first_item_id_be_equal_to_chapter2QuestionId = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[1]).Items[0]).ItemId.ShouldEqual(chapter2QuestionId);

        It should_groupInfoView_second_chapter_first_item_title_be_equal_to_chapter2QuestionTitle = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[1]).Items[0]).Title.ShouldEqual(chapter2QuestionTitle);

        It should_groupInfoView_second_chapter_first_item_variable_be_equal_to_chapter2QuestionVariable = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[1]).Items[0]).Variable.ShouldEqual(chapter2QuestionVariable);

        It should_groupInfoView_second_chapter_first_item_type_be_equal_to_text_question_type = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[1]).Items[0]).Type.ShouldEqual(QuestionType.Text);

        private static string questionnaireId = "33333333333333333333333333333333";
        private static string chapter1Id = "22222222222222222222222222222222";
        private static string chapter2Id = "11111111111111111111111111111111";
        private static string chapter1GroupId = "44444444444444444444444444444444";
        private static string chapter2QuestionId = "55555555555555555555555555555555";
        private static string chapter1Title = "chapter 1 title";
        private static string chapter2Title = "chapter 2 title";
        private static string chapter1GroupTitle = "chapter 1 group title";
        private static string chapter2QuestionTitle = "chapter 2 question title";
        private static string chapter2QuestionVariable = "chapter2textquestion";
        private static string variableUsedInChapter2Question = "var1";
        private static string chapter2QuestionConditionExpression = string.Format("[{0}] > 0", variableUsedInChapter2Question);
        private static string chapter1StaticTextId = "66666666666666666666666666666666";
        private static string chapter1StaticText = "some text";

        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
