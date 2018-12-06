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
    internal class when_initializing : QuestionHeaderViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            substitutedQuesiton = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            substitutionTargetId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
          
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: substitutionTargetId, questionText: "title with %subst%"),
                Create.Entity.TextQuestion(substitutedQuesiton, variable: "subst")
            });

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerTextQuestion(Guid.NewGuid(), substitutedQuesiton, RosterVector.Empty, DateTime.Now, "answer");

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(Moq.It.IsAny<string>()) == interview);

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository);
            BecauseOf();
        }

        public void BecauseOf() => 
            viewModel.Init("interview", Create.Identity(substitutionTargetId, Empty.RosterVector), Create.Other.NavigationState());

        [NUnit.Framework.Test] public void should_substitute_question_titles () => 
            viewModel.Title.HtmlText.Should().Be("title with answer");

        static QuestionHeaderViewModel viewModel;
        private static Guid substitutionTargetId;
        private static Guid substitutedQuesiton;
    }
}

