using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_StaticTextsDeclaredInvalid_received : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            var questionnaireRosterStructure = new Mock<QuestionnaireRosterStructure>();

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure.Object);
            viewState = CreateViewWithSequenceOfInterviewData();

            var identity = Create.Entity.Identity(staticTextId, new decimal[0]);
            var failedValidationConditions = new List<FailedValidationCondition>()
            {
                Create.Entity.FailedValidationCondition(1)
            };
            var listOfFailedValidationConditions = new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>()
            {
                new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(identity, failedValidationConditions)
            };

            publishedEvent = CreatePublishableEvent(new StaticTextsDeclaredInvalid(listOfFailedValidationConditions));
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState, publishedEvent);

        It should_static_text_store_in_cache = () =>
            GetStaticText().ShouldNotBeNull();

        It should_static_text_be_invalid = () =>
            GetStaticText().IsInvalid.ShouldBeTrue();

        private static InterviewStaticText GetStaticText()
        {
            return viewState.Levels["#"].StaticTexts[staticTextId];
        }

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid staticTextId = Guid.Parse("13333333333333333333333333333333");
        private static IPublishedEvent<StaticTextsDeclaredInvalid> publishedEvent;
    }
}
