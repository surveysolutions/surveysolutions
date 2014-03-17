using System;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyDetailsViewDenormalizerTests
{
    internal class when_applying_SupervisorRegistered_event : SurveyDetailsViewDenormalizerTestsContext
    {
        Establish context = () =>
        {
            initialSurveyState = CreateSurveyDetailsView();
            
            @event = ToPublishedEvent(Guid.Parse(surveyShortGuid), new SupervisorRegistered(login, passwordHash));

            denormalizer = CreateSurveyDetailsViewDenormalizer();
        };

        Because of = () =>
            newSurveyState = denormalizer.Update(initialSurveyState, @event);

        It should_return_not_null_view = () =>
            newSurveyState.ShouldNotBeNull();

        It should_return_view_not_null_list_of_supervisors = () =>
            newSurveyState.Supervisors.ShouldNotBeNull();

        It should_return_view_with_one_item_in_supervisors_list = () =>
            newSurveyState.Supervisors.Count.ShouldEqual(1);

        It should_return_view_with_one_supervisors_with_cpercified_login = () =>
            newSurveyState.Supervisors.First().Login.ShouldEqual(login);

        private static SurveyDetailsView initialSurveyState;
        private static SurveyDetailsView newSurveyState;
        private static SurveyDetailsViewDenormalizer denormalizer;
        private static IPublishedEvent<SupervisorRegistered> @event;
        private static string surveyShortGuid = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private static string login = "survey name";
        private static string passwordHash = "==========";
    }
}