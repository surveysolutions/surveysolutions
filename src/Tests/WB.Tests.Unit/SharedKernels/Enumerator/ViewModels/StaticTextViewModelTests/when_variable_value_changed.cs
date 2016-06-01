﻿using System;
using System.Globalization;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_variable_value_changed : StaticTextViewModelTestsContext
    {
        Establish context = () =>
        {
            changedCulture = new ChangeCurrentCulture(CultureInfo.InvariantCulture);
            changedVariables = new[]
            {
                new ChangedVariable(new Identity(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty),  new DateTime(2016, 1, 31)),
                new ChangedVariable(new Identity(Guid.Parse("22222222222222222222222222222222"), RosterVector.Empty),  7.77m),
            };


            var interviewId = "interviewId";
            var staticTextWithSubstitutionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitutedVariable1Name = "var1";
            var substitutedVariable2Name = "var2";
            var substitutedVariable1Identity = new Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            var substitutedVariable2Identity = new Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);

            var answer = new TextAnswer();
            answer.SetAnswer("new value");
            var interview = Mock.Of<IStatefulInterview>(x =>
                x.GetVariableValueByOrDeeperRosterLevel(substitutedVariable1Identity.Id, substitutedVariable1Identity.RosterVector) == changedVariables[0].NewValue &&
                x.GetVariableValueByOrDeeperRosterLevel(substitutedVariable2Identity.Id, substitutedVariable2Identity.RosterVector) == changedVariables[1].NewValue);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireMock = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextWithSubstitutionId, text: $"Your first variable is %{substitutedVariable1Name}% and second is %{substitutedVariable2Name}%"),
                Create.Entity.Variable(variableName: substitutedVariable1Name, id: substitutedVariable1Identity.Id),
                Create.Entity.Variable(variableName: substitutedVariable2Name, id: substitutedVariable2Identity.Id)
            }));

            var questionnaireRepository = new Mock<IPlainQuestionnaireRepository>();
            questionnaireRepository.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireMock);
            
            ILiteEventRegistry registry = Create.Service.LiteEventRegistry();
            liteEventBus = Create.Service.LiteEventBus(registry);

            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository, registry);

            Identity id = new Identity(staticTextWithSubstitutionId, Empty.RosterVector);
            viewModel.Init(interviewId, id, null);

            fakeInterview = Create.AggregateRoot.Interview();
        };

        Because of = () => liteEventBus.PublishCommittedEvents(new CommittedEventStream(fakeInterview.EventSourceId, 
            Create.Other.CommittedEvent(payload:new VariablesChanged(changedVariables), eventSourceId: fakeInterview.EventSourceId)));

        It should_change_item_title = () => viewModel.StaticText.ShouldEqual("Your first variable is 01/31/2016 and second is 7.77");

        Cleanup cleanup = () => changedCulture.Dispose();

        static StaticTextViewModel viewModel;
        static ILiteEventBus liteEventBus;
        static IEventSourcedAggregateRoot fakeInterview;
        private static ChangedVariable[] changedVariables;
        public static ChangeCurrentCulture changedCulture;
    }
}

