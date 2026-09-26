using System;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    [TestFixture]
    internal class when_route_values_are_malformed : QuestionnaireApiControllerTestContext
    {
        [Test]
        public void should_return_not_found_for_null_questionnaire_in_chapter_and_not_call_factory()
        {
            var chapterInfoViewFactory = new Mock<IChapterInfoViewFactory>();
            var controller = CreateQuestionnaireController(chapterInfoViewFactory: chapterInfoViewFactory.Object);

            var actionResult = controller.Chapter(null, validChapterId);

            Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
            chapterInfoViewFactory.Verify(x => x.Load(It.IsAny<QuestionnaireRevision>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void should_return_not_found_for_invalid_chapter_id_and_not_call_factory()
        {
            var chapterInfoViewFactory = new Mock<IChapterInfoViewFactory>();
            var controller = CreateQuestionnaireController(chapterInfoViewFactory: chapterInfoViewFactory.Object);

            var actionResult = controller.Chapter(questionnaireId, "not-a-guid");

            Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
            chapterInfoViewFactory.Verify(x => x.Load(It.IsAny<QuestionnaireRevision>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void should_return_not_found_for_null_questionnaire_in_edit_variable_and_not_call_factory()
        {
            var questionnaireInfoFactory = new Mock<IQuestionnaireInfoFactory>();
            var controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoFactory.Object);

            var actionResult = controller.EditVariable(null, itemId);

            Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
            questionnaireInfoFactory.Verify(x => x.GetVariableEditView(It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void should_return_not_found_for_null_questionnaire_in_edit_question_and_not_call_factory()
        {
            var questionnaireInfoFactory = new Mock<IQuestionnaireInfoFactory>();
            var controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoFactory.Object);

            var actionResult = controller.EditQuestion(null, itemId);

            Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
            questionnaireInfoFactory.Verify(x => x.GetQuestionEditView(It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void should_return_not_found_for_null_questionnaire_in_edit_group_and_not_call_factory()
        {
            var questionnaireInfoFactory = new Mock<IQuestionnaireInfoFactory>();
            var controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoFactory.Object);

            var actionResult = controller.EditGroup(null, itemId);

            Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
            questionnaireInfoFactory.Verify(x => x.GetGroupEditView(It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void should_return_not_found_for_null_questionnaire_in_edit_roster_and_not_call_factory()
        {
            var questionnaireInfoFactory = new Mock<IQuestionnaireInfoFactory>();
            var controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoFactory.Object);

            var actionResult = controller.EditRoster(null, itemId);

            Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
            questionnaireInfoFactory.Verify(x => x.GetRosterEditView(It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void should_return_not_found_for_null_questionnaire_in_edit_static_text_and_not_call_factory()
        {
            var questionnaireInfoFactory = new Mock<IQuestionnaireInfoFactory>();
            var controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoFactory.Object);

            var actionResult = controller.EditStaticText(null, itemId);

            Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
            questionnaireInfoFactory.Verify(x => x.GetStaticTextEditView(It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void should_return_not_found_for_null_questionnaire_in_verify_and_not_call_factory()
        {
            var questionnaireViewFactory = new Mock<IQuestionnaireViewFactory>();
            var controller = CreateQuestionnaireController(questionnaireViewFactory: questionnaireViewFactory.Object);

            var actionResult = controller.Verify(null);

            Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
            questionnaireViewFactory.Verify(x => x.Load(It.IsAny<QuestionnaireRevision>()), Times.Never);
        }

        private static readonly Guid itemId = Guid.Parse("11111111111111111111111111111111");
        private const string validChapterId = "22222222222222222222222222222222";
    }
}
