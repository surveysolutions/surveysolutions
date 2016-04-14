using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestFixture]
    public class StaticTextViewModelTests
    {
        private Identity staticTextIdentity;
        private QuestionnaireIdentity questionnaireIdentity;

        [SetUp]
        public void Setup()
        {
            staticTextIdentity = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);
            questionnaireIdentity = Create.QuestionnaireIdentity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), 1);
        }

        [Test]
        public async Task when_static_text_is_disabled_should_strip_html_tags()
        {
            var interview = Mock.Of<IStatefulInterview>(x => x.QuestionnaireIdentity == questionnaireIdentity);
            var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetStaticText(staticTextIdentity.Id) == "text with <b>html</b>");

            var questionnaireRepository = Create.PlainQuestionnaireRepositoryWith(questionnaire);
            var interviewRepository = Create.StatefulInterviewRepositoryWith(interview);

            var viewModel = CreateViewModel(questionnaireRepository, interviewRepository);

            await viewModel.InitAsync("id", staticTextIdentity, Create.NavigationState());

            Assert.That(viewModel.StaticText, Is.EqualTo("text with html"));
        }

        [Test]
        public async Task when_static_text_is_enabled_should_put_html_as_is()
        {
            var interview = Mock.Of<IStatefulInterview>(x => x.QuestionnaireIdentity == questionnaireIdentity && x.IsEnabled(this.staticTextIdentity) == true);
            var textWithHtml = "text with <b>html</b>";
            var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetStaticText(staticTextIdentity.Id) == textWithHtml);

            var questionnaireRepository = Create.PlainQuestionnaireRepositoryWith(questionnaire);
            var interviewRepository = Create.StatefulInterviewRepositoryWith(interview);

            var viewModel = CreateViewModel(questionnaireRepository, interviewRepository);

            await viewModel.InitAsync("id", staticTextIdentity, Create.NavigationState());

            Assert.That(viewModel.StaticText, Is.EqualTo(textWithHtml));
        }

        [Test]
        public async Task when_static_text_is_invalid_should_strip_html()
        {
            var interview = Mock.Of<IStatefulInterview>(x => x.QuestionnaireIdentity == questionnaireIdentity && x.IsValid(this.staticTextIdentity) == false);
            var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetStaticText(staticTextIdentity.Id) == "text with <b>html</b>");

            var questionnaireRepository = Create.PlainQuestionnaireRepositoryWith(questionnaire);
            var interviewRepository = Create.StatefulInterviewRepositoryWith(interview);

            var viewModel = CreateViewModel(questionnaireRepository, interviewRepository);

            await viewModel.InitAsync("id", staticTextIdentity, Create.NavigationState());

            Assert.That(viewModel.StaticText, Is.EqualTo("text with html"));
        }

        [Test]
        public async Task when_static_text_is_valid_and_enabled_should_put_html_as_is()
        {
            var interview = Mock.Of<IStatefulInterview>(x => x.QuestionnaireIdentity == questionnaireIdentity && 
                                                             x.IsValid(this.staticTextIdentity) == true &&
                                                             x.IsEnabled(this.staticTextIdentity) == true);
            var textWithHtml = "text with <b>html</b>";
            var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetStaticText(staticTextIdentity.Id) == textWithHtml);

            var questionnaireRepository = Create.PlainQuestionnaireRepositoryWith(questionnaire);
            var interviewRepository = Create.StatefulInterviewRepositoryWith(interview);

            var viewModel = CreateViewModel(questionnaireRepository, interviewRepository);

            await viewModel.InitAsync("id", staticTextIdentity, Create.NavigationState());

            Assert.That(viewModel.StaticText, Is.EqualTo(textWithHtml));
        }

        static StaticTextViewModel CreateViewModel(IPlainQuestionnaireRepository questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            AttachmentViewModel attachmentViewModel = null,
            StaticTextStateViewModel questionState = null)
        {
            return new StaticTextViewModel(
            questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
            interviewRepository ?? Mock.Of< IStatefulInterviewRepository>(),
            attachmentViewModel ?? Create.AttachmentViewModel(questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(), 
                                                              interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                                                              Mock.Of<IAttachmentContentStorage>()),
            questionState ?? Create.ViewModels.StaticTextStateViewModel(interviewRepository ?? Mock.Of<IStatefulInterviewRepository>()));
        }
    }
}