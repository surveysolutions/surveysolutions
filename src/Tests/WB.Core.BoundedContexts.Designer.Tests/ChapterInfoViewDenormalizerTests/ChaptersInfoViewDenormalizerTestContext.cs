﻿using System.Collections.Generic;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.BoundedContexts.Designer.Tests.ChapterInfoViewDenormalizerTests
{
    internal class ChaptersInfoViewDenormalizerTestContext
    {
        protected static ChaptersInfoViewDenormalizer CreateDenormalizer(GroupInfoView view = null, IExpressionProcessor expressionProcessor = null)
        {
            var readSideRepositoryWriter = new Mock<IReadSideRepositoryWriter<GroupInfoView>>();
            readSideRepositoryWriter.Setup(x => x.GetById(It.IsAny<string>())).Returns(view);

            return new ChaptersInfoViewDenormalizer(readSideRepositoryWriter.Object,
                expressionProcessor ?? Mock.Of<IExpressionProcessor>());
        }

        protected static GroupInfoView CreateGroupInfoView()
        {
            return new GroupInfoView() {Items = new List<IQuestionnaireItem>()};
        }

        protected static GroupInfoView CreateGroupInfoViewWith1Chapter(string chapterId)
        {
            var questionnaireInfoView = CreateGroupInfoView();
            questionnaireInfoView.Items.Add(new GroupInfoView() {ItemId = chapterId, Items = new List<IQuestionnaireItem>()});

            return questionnaireInfoView;
        }

        protected static GroupInfoView CreateGroupInfoViewWith1ChapterAnd1GroupInsideChapter(string chapterId, string groupId)
        {
            var questionnaireInfoView = CreateGroupInfoViewWith1Chapter(chapterId);
            ((GroupInfoView)questionnaireInfoView.Items[0]).Items.Add(new GroupInfoView() { ItemId = groupId, Items = new List<IQuestionnaireItem>() });

            return questionnaireInfoView;
        }


        protected static GroupInfoView CreateGroupInfoViewWith1ChapterAnd1QuestionInsideChapter(string chapterId, string questionId)
        {
            var questionnaireInfoView = CreateGroupInfoViewWith1Chapter(chapterId);
            ((GroupInfoView) questionnaireInfoView.Items[0]).Items.Add(new QuestionInfoView() {ItemId = questionId});

            return questionnaireInfoView;
        }

        protected static GroupInfoView CreateGroupInfoViewWith2ChaptersAndQuestionsInThem(string chapter1Id,
            string chapter2Id, string question1Id, string question2Id)
        {
            var questionnaireInfoView = CreateGroupInfoViewWith1ChapterAnd1QuestionInsideChapter(chapter1Id, question1Id);
            questionnaireInfoView.Items.Add(new GroupInfoView()
            {
                ItemId = chapter2Id,
                Items = new List<IQuestionnaireItem>()
                {
                    new QuestionInfoView() {ItemId = question2Id}
                }
            });

            return questionnaireInfoView;
        }

        protected static GroupInfoView CreateGroupInfoViewWith2ChaptersAndGroupsInThem(string chapter1Id,
            string chapter2Id, string group1Id, string group2Id)
        {
            var questionnaireInfoView = CreateGroupInfoViewWith1ChapterAnd1GroupInsideChapter(chapter1Id, group1Id);
            questionnaireInfoView.Items.Add(new GroupInfoView()
            {
                ItemId = chapter2Id,
                Items = new List<IQuestionnaireItem>()
                {
                    new GroupInfoView() {ItemId = group2Id}
                }
            });

            return questionnaireInfoView;
        }
    }
}
