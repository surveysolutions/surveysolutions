using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_roster_title_substitution_is_used : QuestionHeaderViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            substitutionTargetQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.FixedRoster(children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: substitutionTargetQuestionId, questionText: "title with %rostertitle%")
                })
            });

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(Moq.It.IsAny<string>()) == interview);

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository);
            BecauseOf();
        }

        public void BecauseOf() => 
            viewModel.Init("interview", Create.Identity(substitutionTargetQuestionId, Create.Entity.RosterVector(0)));

        [NUnit.Framework.Test] public void should_substitute_roster_title_value () => 
            viewModel.Title.HtmlText.Should().Be("title with Fixed Roster 1");

        static QuestionHeaderViewModel viewModel;
        static Guid substitutionTargetQuestionId;
    }
}
