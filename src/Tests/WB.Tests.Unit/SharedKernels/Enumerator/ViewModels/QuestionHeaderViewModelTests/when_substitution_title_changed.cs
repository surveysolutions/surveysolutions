using System;
using FluentAssertions;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_substitution_title_changed : QuestionHeaderViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionWithSubstitutionIdentity = Identity.Create(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);

            var substitutedVariable = "var1";

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionWithSubstitutionIdentity.Id, text: $"Old title %{substitutedVariable}%"),
                Create.Entity.TextQuestion(variable: substitutedVariable, questionId: substitutedQuestionId)
            );

            statefullInterview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            statefullInterview.AnswerTextQuestion(interviewerId, substitutedQuestionId, RosterVector.Empty, DateTime.UtcNow, "new value");

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefullInterview);

            ILiteEventRegistry registry = Create.Service.LiteEventRegistry();
            liteEventBus = Create.Service.LiteEventBus(registry);

            viewModel = CreateViewModel(Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire), interviewRepository, registry);
            viewModel.Init("interview", questionWithSubstitutionIdentity, Create.Other.NavigationState());
            BecauseOf();
        }

        public void BecauseOf() => liteEventBus.PublishCommittedEvents(new CommittedEventStream(statefullInterview.EventSourceId,
            Create.Other.CommittedEvent(payload: Create.Event.SubstitutionTitlesChanged(questions: new Identity[]
                {
                    Create.Identity(substitutedQuestionId, Empty.RosterVector)
                }), eventSourceId: statefullInterview.EventSourceId)));

        [NUnit.Framework.Test] public void should_change_item_title () => viewModel.Title.HtmlText.Should().Be("Old title new value");

        static ILiteEventBus liteEventBus;
        static QuestionHeaderViewModel viewModel;
        static StatefulInterview statefullInterview;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
        static Guid substitutedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Identity questionWithSubstitutionIdentity;
    }
}

