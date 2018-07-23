using System;
using System.Globalization;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_variable_value_changed_and_current_culture_is_russian : StaticTextViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var substitutedVariable1Identity = Create.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            var substitutedVariable2Identity = Create.Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);
            staticTextIdentity = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
            var substitutedVariable1Name = "var1";
            var substitutedVariable2Name = "var2";

            changedCulture = new ChangeCurrentCulture(new CultureInfo("ru-Ru"));

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextIdentity.Id, text: $"Your first variable is %{substitutedVariable1Name}% and second is %{substitutedVariable2Name}%"),
                Create.Entity.Variable(variableName: substitutedVariable1Name, id: substitutedVariable1Identity.Id),
                Create.Entity.Variable(variableName: substitutedVariable2Name, id: substitutedVariable2Identity.Id)
            });

            interview = Create.AggregateRoot.StatefulInterview(questionnaire.PublicKey, userId: Guid.NewGuid(), questionnaire: questionnaire);
            interview.Apply(Create.Event.VariablesChanged(new[]
            {
                new ChangedVariable(substitutedVariable1Identity,  new DateTime(2016, 1, 31)),
                new ChangedVariable(substitutedVariable2Identity,  7.77m),
            }));
            interview.Apply(Create.Event.SubstitutionTitlesChanged(staticTexts: new[] { staticTextIdentity }));

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interview.EventSourceId.FormatGuid()) == interview);

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            ILiteEventRegistry registry = Create.Service.LiteEventRegistry();
            liteEventBus = Create.Service.LiteEventBus(registry);

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository, registry);
            viewModel.Init(interview.EventSourceId.FormatGuid(), staticTextIdentity, null);
            BecauseOf();
        }

        public void BecauseOf() => 
            liteEventBus.PublishCommittedEvents(new CommittedEventStream(interview.EventSourceId, 
            Create.Other.CommittedEvent(payload: Create.Event.SubstitutionTitlesChanged(staticTexts: new [] { staticTextIdentity }), eventSourceId: interview.EventSourceId)));

        [NUnit.Framework.Test] public void should_change_item_title () => 
            viewModel.Text.PlainText.Should().Be("Your first variable is 2016-01-31 and second is 7,77");

        [NUnit.Framework.OneTimeTearDown]
        public void TearDown() => changedCulture.Dispose();

        static StaticTextViewModel viewModel;
        static ILiteEventBus liteEventBus;
        static StatefulInterview interview;
        public static ChangeCurrentCulture changedCulture;
        private static Identity staticTextIdentity;
    }
}

