﻿using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.ChapterInfoViewDenormalizerTests
{
    internal class when_QRBarcodeQuestionCloned_event_received : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            var expressionProcessor = new Mock<IExpressionProcessor>();
            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(questionConditionExpression))
                .Returns(new[] { variableUsedInQuestionConditionExpression });

            viewState = CreateGroupInfoViewWith1ChapterAnd1QuestionInsideChapter(chapterId, sourceQuestionId);
            denormalizer = CreateDenormalizer(view: viewState, expressionProcessor: expressionProcessor.Object);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState,
                    CreatePublishableEvent(new QRBarcodeQuestionCloned()
                    {
                        QuestionId = Guid.Parse(questionId),
                        ParentGroupId = Guid.Parse(chapterId),
                        VariableName = questionVariable,
                        Title = questionTitle,
                        EnablementCondition = questionConditionExpression,
                        SourceQuestionId = Guid.Parse(sourceQuestionId),
                        TargetIndex = 0
                    }));

        It should_groupInfoView_first_chapter_items_not_be_null = () =>
            ((GroupInfoView)viewState.Items[0]).Items.ShouldNotBeNull();

        It should_groupInfoView_first_chapter_items_count_be_equal_to_2 = () =>
            ((GroupInfoView)viewState.Items[0]).Items.Count.ShouldEqual(2);

        It should_groupInfoView_first_chapter_first_item_type_be_equal_to_QuestionInfoView = () =>
            ((GroupInfoView)viewState.Items[0]).Items[0].ShouldBeOfExactType<QuestionInfoView>();

        It should_groupInfoView_first_chapter_first_item_id_be_equal_to_questionId = () =>
            ((GroupInfoView)viewState.Items[0]).Items[0].ItemId.ShouldEqual(questionId);

        It should_groupInfoView_first_chapter_first_question_title_be_equal_to_questionTitle = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[0]).Items[0]).Title.ShouldEqual(questionTitle);

        It should_groupInfoView_first_chapter_first_question_variable_be_equal_to_questionVariable = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[0]).Items[0]).Variable.ShouldEqual(questionVariable);

        It should_groupInfoView_first_chapter_first_question_type_be_equal_to_questionType = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[0]).Items[0]).Type.ShouldEqual(QuestionType.QRBarcode);

        It should_groupInfoView_first_chapter_first_question_LinkedVariables_not_be_null = () =>
            ((QuestionInfoView)((GroupInfoView)viewState.Items[0]).Items[0]).LinkedVariables.ShouldNotBeNull();

        It should_groupInfoView_first_chapter_first_question_LinkedVariables_have_1_variable = () =>
            ((QuestionInfoView) ((GroupInfoView) viewState.Items[0]).Items[0]).LinkedVariables.Count().ShouldEqual(1);

        It should_groupInfoView_first_chapter_first_question_LinkedVariables_first_variable_be_equal_to_variableUsedInQuestionConditionExpression = () =>
            ((QuestionInfoView) ((GroupInfoView) viewState.Items[0]).Items[0]).LinkedVariables.First().ShouldEqual(variableUsedInQuestionConditionExpression);

        private static string chapterId = "33333333333333333333333333333333";
        private static string questionId = "22222222222222222222222222222222";
        private static string sourceQuestionId = "11111111111111111111111111111111";
        private static string questionTitle = "question title";
        private static string questionVariable = "var";
        private static string variableUsedInQuestionConditionExpression = "var1";
        private static string questionConditionExpression = string.Format("[{0}] > 0", variableUsedInQuestionConditionExpression);
        
        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
