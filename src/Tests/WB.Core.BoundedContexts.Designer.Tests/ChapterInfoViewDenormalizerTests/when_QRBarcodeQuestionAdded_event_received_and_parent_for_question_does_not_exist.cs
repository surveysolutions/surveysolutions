﻿using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.ChapterInfoViewDenormalizerTests
{
    internal class when_QRBarcodeQuestionAdded_event_received_and_parent_for_question_does_not_exist : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateGroupInfoViewWith1Chapter(chapterId);
            denormalizer = CreateDenormalizer(view: viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState,
                    CreatePublishableEvent(new QRBarcodeQuestionAdded()
                    {
                        QuestionId = Guid.Parse(questionId),
                        ParentGroupId = Guid.Parse(notExistingGroupId)
                    }));

        It should_groupInfoView_first_chapter_items_be_empty = () =>
            ((GroupInfoView)viewState.Items[0]).Items.ShouldBeEmpty();


        private static string chapterId = "33333333333333333333333333333333";
        private static string questionId = "22222222222222222222222222222222";
        private static string notExistingGroupId = "11111111111111111111111111111111";
        
        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
