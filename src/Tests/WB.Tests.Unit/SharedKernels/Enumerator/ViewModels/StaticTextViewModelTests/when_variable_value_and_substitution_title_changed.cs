using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_variable_value_and_substitution_title_changed : StaticTextViewModelTestsContext
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            var staticTextWithSubstitutionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            staticTextIdentity = Create.Identity(staticTextWithSubstitutionId, Empty.RosterVector);
            var substitutedVariableIdentity =
                Create.Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextWithSubstitutionId,
                    text: $"Your answer on question is %var1% and variable is %var2%"),
                Create.Entity.NumericRealQuestion(variable: "var1", id: substitutedQuestionId),
                Create.Entity.Variable(variableName: "var2", id: substitutedVariableIdentity.Id,
                    expression: $"(var1*100)/20")
            });

            statefullInterview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire,
                setupLevel: level =>
                {
                    level.Setup(x => x.GetVariableExpression(Create.Identity(substitutedVariableIdentity.Id)))
                        .Returns(() => 10);
                });
            var interviewRepository = Setup.StatefulInterviewRepository(statefullInterview);

            ILiteEventRegistry registry = Create.Service.LiteEventRegistry();
            liteEventBus = Create.Service.LiteEventBus(registry);

            viewModel = CreateViewModel(Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire),
                interviewRepository, registry);
            viewModel.Init(statefullInterview.EventSourceId.FormatGuid(), staticTextIdentity, null);

            statefullInterview.AnswerNumericRealQuestion(interviewerId, substitutedQuestionId, RosterVector.Empty,
                DateTime.UtcNow, 2);
            statefullInterview.Apply(
                Create.Event.VariablesChanged(DateTimeOffset.Now, new ChangedVariable(substitutedVariableIdentity, 10)));
            statefullInterview.Apply(Create.Event.SubstitutionTitlesChanged(staticTexts: new[] {staticTextIdentity}));

            liteEventBus.PublishCommittedEvents(new CommittedEventStream(statefullInterview.EventSourceId,
                Create.Other.CommittedEvent(
                    payload: Create.Event.SubstitutionTitlesChanged(staticTexts: new[] {staticTextIdentity}),
                    eventSourceId: statefullInterview.EventSourceId)));
        }

        [Test]
        public void should_change_item_title() => 
            viewModel.Text.PlainText.Should().Be($"Your answer on question is 2 and variable is 10");

        static StaticTextViewModel viewModel;
        static StatefulInterview statefullInterview;
        static ILiteEventBus liteEventBus;
        static Identity staticTextIdentity;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
        static Guid substitutedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}

