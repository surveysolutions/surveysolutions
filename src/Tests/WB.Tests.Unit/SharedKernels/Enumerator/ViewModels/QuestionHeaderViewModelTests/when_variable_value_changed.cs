using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_variable_value_changed : QuestionHeaderViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var substitutedVariable1Identity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);
            var substitutedVariable2Identity = Create.Identity(Guid.Parse("22222222222222222222222222222222"), RosterVector.Empty);;
            var substitutedVariable1Name = "var1";
            var substitutedVariable2Name = "var2";
            
            substitutionTargetQuestionIdentity = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);

            var questionnaireMock = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.TextQuestion(substitutionTargetQuestionIdentity.Id, text: $"Your first variable is %{substitutedVariable1Name}% and second is %{substitutedVariable2Name}%"),
                Create.Entity.Variable(substitutedVariable1Identity.Id, VariableType.DateTime, substitutedVariable1Name),
                Create.Entity.Variable(substitutedVariable2Identity.Id, VariableType.Double, substitutedVariable2Name),
            });

            interview = Setup.StatefulInterview(questionnaireMock);
            interview.Apply(Create.Event.VariablesChanged(DateTimeOffset.Now, new[]
            {
                new ChangedVariable(substitutedVariable1Identity,  new DateTime(2016, 1, 31)),
                new ChangedVariable(substitutedVariable2Identity,  7.77m),
            }));
            interview.Apply(Create.Event.SubstitutionTitlesChanged(questions: new[] { substitutionTargetQuestionIdentity }));

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireMock);
           
            viewModel = CreateViewModel(questionnaireRepository, interviewRepository);
            BecauseOf();
        }

        public void BecauseOf() => viewModel.Init("interview", substitutionTargetQuestionIdentity);

        [NUnit.Framework.Test] public void should_change_item_title () => viewModel.Title.HtmlText.Should().Be("Your first variable is 2016-01-31 and second is 7.77");

        static QuestionHeaderViewModel viewModel;
        static StatefulInterview interview;
        static Identity substitutionTargetQuestionIdentity;
    }
}

