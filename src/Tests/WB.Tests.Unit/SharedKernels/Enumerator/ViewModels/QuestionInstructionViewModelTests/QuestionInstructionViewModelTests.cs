using System;
using FluentAssertions;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionInstructionViewModelTests
{
    public class QuestionInstructionViewModelTests : QuestionInstructionViewModelTestContext
    {
        [Test]
        public void when_instructions_substitution_changed1()
        {
            Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
            Guid substitutedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionWithSubstitutionIdentity = Identity.Create(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);

            var substitutedVariable = "var1";

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionWithSubstitutionIdentity.Id, instruction: $"Old title %{substitutedVariable}%"),
                Create.Entity.TextQuestion(variable: substitutedVariable, questionId: substitutedQuestionId)
            );

            var statefullInterview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            statefullInterview.AnswerTextQuestion(interviewerId, substitutedQuestionId, RosterVector.Empty, DateTime.UtcNow, "new value");

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefullInterview);

            ILiteEventRegistry registry = Create.Service.LiteEventRegistry();
            ILiteEventBus liteEventBus = Create.Service.LiteEventBus(registry);

            var viewModel = CreateQuestionHeaderViewModel(Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire), interviewRepository, registry);
            viewModel.Init("interview", questionWithSubstitutionIdentity);

            liteEventBus.PublishCommittedEvents(new CommittedEventStream(statefullInterview.EventSourceId,
                Create.Other.CommittedEvent(payload: Create.Event.SubstitutionTitlesChanged(questions: new Identity[]
                {
                    Create.Identity(substitutedQuestionId, Empty.RosterVector)
                }), eventSourceId: statefullInterview.EventSourceId)));

            viewModel.Instruction.HtmlText.Should().Be("Old title new value");
        }
    }
}
