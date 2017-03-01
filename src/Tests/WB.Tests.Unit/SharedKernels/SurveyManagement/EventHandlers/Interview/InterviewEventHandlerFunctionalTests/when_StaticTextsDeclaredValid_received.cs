﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_StaticTextsDeclaredValid_received : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            var questionnaireRosterStructure = new Mock<Dictionary<ValueVector<Guid>, RosterScopeDescription>>();

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure.Object);
            viewState = CreateViewWithSequenceOfInterviewData();

            var identity = Create.Entity.Identity(staticTextId, new decimal[0]);

            publishedEvent = CreatePublishableEvent(new StaticTextsDeclaredValid(new[] {identity}));

            viewState.Levels["#"].StaticTexts.Add(staticTextId, new InterviewStaticText(staticTextId) { IsInvalid = true });
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState, publishedEvent);

        It should_static_text_store_in_cache = () =>
            GetStaticText().ShouldNotBeNull();

        It should_static_text_be_valid = () =>
            GetStaticText().IsInvalid.ShouldBeFalse();

        private static InterviewStaticText GetStaticText()
        {
            return viewState.Levels["#"].StaticTexts[staticTextId];
        }

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid staticTextId = Guid.Parse("13333333333333333333333333333333");
        private static IPublishedEvent<StaticTextsDeclaredValid> publishedEvent;
    }
}
