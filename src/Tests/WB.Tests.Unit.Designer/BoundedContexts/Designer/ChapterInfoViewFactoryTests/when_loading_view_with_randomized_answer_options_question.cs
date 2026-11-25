using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ChapterInfoViewFactoryTests
{
    internal class when_loading_view_with_randomized_answer_options_question : ChapterInfoViewFactoryContext
    {
        [Test]
        public void should_mark_only_question_with_randomized_answer_options()
        {
            var randomizedQuestionId = Guid.Parse("33333333333333333333333333333333");
            var plainQuestionId = Guid.Parse("44444444444444444444444444444444");

            var randomizedQuestion = Create.SingleOptionQuestion(randomizedQuestionId, variable: "randomized");
            randomizedQuestion.RandomizeAnswerOptions = true;

            var repositoryMock = new Mock<IDesignerQuestionnaireStorage>();
            repositoryMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(Create.QuestionnaireDocumentWithOneChapter(chapterId,
                    randomizedQuestion,
                    Create.SingleOptionQuestion(plainQuestionId, variable: "plain")));

            var factory = CreateChapterInfoViewFactory(repository: repositoryMock.Object);

            var view = factory.Load(questionnaireId, chapterId.FormatGuid());

            var questions = view.Chapter.Items.OfType<QuestionInfoView>().ToDictionary(x => x.ItemId);
            questions[randomizedQuestionId.FormatGuid()].RandomizeAnswerOptions.Should().BeTrue();
            questions[plainQuestionId.FormatGuid()].RandomizeAnswerOptions.Should().BeFalse();
        }

        private static readonly QuestionnaireRevision questionnaireId = Create.QuestionnaireRevision("11111111111111111111111111111111");
        private static readonly Guid chapterId = Guid.Parse("22222222222222222222222222222222");
    }
}
