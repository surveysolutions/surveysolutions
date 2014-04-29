using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.ChapterInfoViewDenormalizerTests
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
            viewState = denormalizer.Create(
                CreatePublishableEvent(new TemplateImported()
                {
                    Source =
                        new QuestionnaireDocument()
                        {
                            PublicKey = Guid.Parse(questionnaireId),
                            Children = new List<IComposite>()
                            {
                                new Group()
                                {
                                    PublicKey = Guid.Parse(chapter1Id),
                                    Title = chapter1Title,
                                    Children = new List<IComposite>()
                                    {
                                        new Group()
                                        {
                                            PublicKey = Guid.Parse(chapter1GroupId),
                                            Title = chapter1GroupTitle
                                        }
                                    }
                                },
                                new Group()
                                {
                                    PublicKey = Guid.Parse(chapter2Id),
                                    Title = chapter2Title,
                                    Children = new List<IComposite>()
                                    {
                                        new TextQuestion()
                                        {
                                            PublicKey = Guid.Parse(chapter2QuestionId),
                                            QuestionText = chapter2QuestionTitle,
                                            StataExportCaption = chapter2QuestionVariable,
                                            QuestionType = chapter2QuestionType,
                                            ConditionExpression = chapter2QuestionConditionExpression
                                        }
                                    }
                                }
                            }
                        }
                }, new Guid(questionnaireId)));

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

        It should_groupInfoView_first_chapter_contains_1_item = () =>
            ((GroupInfoView)viewState.Items[0]).Items.Count.ShouldEqual(1);

        It should_groupInfoView_first_chapter_first_item_be_type_of_GroupInfoView = () =>
            ((GroupInfoView)viewState.Items[0]).Items[0].ShouldBeOfExactType<GroupInfoView>();

        It should_groupInfoView_first_chapter_first_item_id_be_equal_to_chapter1GroupId = () =>
            ((GroupInfoView)((GroupInfoView)viewState.Items[0]).Items[0]).ItemId.ShouldEqual(chapter1GroupId);

        It should_groupInfoView_first_chapter_first_item_title_be_equal_to_chapter1GroupTitle = () =>
            ((GroupInfoView)((GroupInfoView)viewState.Items[0]).Items[0]).Title.ShouldEqual(chapter1GroupTitle);

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

        It should_groupInfoView_second_chapter_first_item_type_be_equal_to_chapter2QuestionType = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[1]).Items[0]).Type.ShouldEqual(chapter2QuestionType);

        It should_groupInfoView_second_chapter_first_item_LinkedVariables_not_be_null = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[1]).Items[0]).LinkedVariables.ShouldNotBeNull();

        It should_groupInfoView_second_chapter_first_item_LinkedVariables_contains_1_variable = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[1]).Items[0]).LinkedVariables.Count().ShouldEqual(1);

        It should_groupInfoView_second_chapter_first_item_LinkedVariables_first_variable_be_equal_to_variableUsedInChapter2Question = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[1]).Items[0]).LinkedVariables.FirstOrDefault(lv=> lv == variableUsedInChapter2Question).ShouldNotBeNull();

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
        private static QuestionType chapter2QuestionType = QuestionType.Text;
        private static string variableUsedInChapter2Question = "var1";
        private static string chapter2QuestionConditionExpression = string.Format("[{0}] > 0", variableUsedInChapter2Question);

        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
