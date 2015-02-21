using System.Collections.Generic;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.BoundedContexts.Designer.ChapterInfoViewDenormalizerTests
{
    internal class ChaptersInfoViewDenormalizerTestContext
    {
        protected static ChaptersInfoViewDenormalizer CreateDenormalizer(GroupInfoView view = null, IExpressionProcessor expressionProcessor = null)
        {
            var readSideRepositoryWriter = new Mock<IReadSideKeyValueStorage<GroupInfoView>>();
            readSideRepositoryWriter.Setup(x => x.GetById(It.IsAny<string>())).Returns(view);

            return new ChaptersInfoViewDenormalizer(readSideRepositoryWriter.Object);
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

        protected static GroupInfoView CreateGroupInfoViewWith1ChapterAnd1RosterInsideChapter(string chapterId, string rosterId)
        {
            var questionnaireInfoView = CreateGroupInfoViewWith1Chapter(chapterId);
            ((GroupInfoView) questionnaireInfoView.Items[0]).Items.Add(new GroupInfoView()
            {
                ItemId = rosterId,
                Items = new List<IQuestionnaireItem>(),
                IsRoster = true
            });

            return questionnaireInfoView;
        }


        protected static GroupInfoView CreateGroupInfoViewWith1ChapterAnd1QuestionInsideChapter(string chapterId, string questionId)
        {
            var questionnaireInfoView = CreateGroupInfoViewWith1Chapter(chapterId);
            ((GroupInfoView) questionnaireInfoView.Items[0]).Items.Add(new QuestionInfoView() {ItemId = questionId});

            return questionnaireInfoView;
        }

        protected static GroupInfoView CreateGroupInfoViewWith1ChapterAnd1StaticTextInsideChapter(string chapterId, string entityId)
        {
            var questionnaireInfoView = CreateGroupInfoViewWith1Chapter(chapterId);
            ((GroupInfoView)questionnaireInfoView.Items[0]).Items.Add(new StaticTextInfoView() { ItemId = entityId });

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
