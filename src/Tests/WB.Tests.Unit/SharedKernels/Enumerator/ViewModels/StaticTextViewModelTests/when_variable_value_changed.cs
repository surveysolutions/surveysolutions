using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_variable_value_changed : StaticTextViewModelTestsContext
    {
        [Test]
        [SetCulture("")]
        public void should_change_item_title()
        {
            var staticTextIdentity = Id.IdentityA;
            var substitutedVariable1Name = "var1";
            var substitutedVariable2Name = "var2";
            var substitutedVariable1Identity =
                Create.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            var substitutedVariable2Identity =
                Create.Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextIdentity.Id,
                    text:
                    $"Your first variable is %{substitutedVariable1Name}% and second is %{substitutedVariable2Name}%"),
                Create.Entity.Variable(variableName: substitutedVariable1Name, id: substitutedVariable1Identity.Id),
                Create.Entity.Variable(variableName: substitutedVariable2Name, id: substitutedVariable2Identity.Id)
            });

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire.PublicKey, userId: Guid.NewGuid(),
                questionnaire: questionnaire);
            interview.Apply(Create.Event.VariablesChanged(new[]
            {
                new ChangedVariable(substitutedVariable1Identity, new DateTime(2016, 1, 31)),
                new ChangedVariable(substitutedVariable2Identity, 7.77m),
            }));
            interview.Apply(Create.Event.SubstitutionTitlesChanged(staticTexts: new[] {staticTextIdentity}));

            var interviewRepository =
                Create.Storage.InterviewRepository(interview);

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            IViewModelEventRegistry registry = Create.Service.LiteEventRegistry();
            var liteEventBus = Create.Service.LiteEventBus(registry);

            var viewModel = CreateViewModel(questionnaireRepository, interviewRepository, registry);

            viewModel.Init(interview.EventSourceId.FormatGuid(), staticTextIdentity, Create.Other.NavigationState());

            // Act
            liteEventBus.PublishCommittedEvents(new CommittedEventStream(interview.EventSourceId,
                Create.Other.CommittedEvent(
                    payload: Create.Event.SubstitutionTitlesChanged(staticTexts: new[] {staticTextIdentity}),
                    eventSourceId: interview.EventSourceId)));

            // Assert
            viewModel.Text.PlainText.Should().Be("Your first variable is 2016-01-31 and second is 7.77");
        }
    }
}
