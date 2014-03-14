using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers
{
    internal class SurveyDetailsViewDenormalizer : AbstractFunctionalEventHandler<SurveyDetailsView>
        , IUpdateHandler<SurveyDetailsView, NewSurveyStarted>
        , IUpdateHandler<SurveyDetailsView, SupervisorRegistered>
    {
        public SurveyDetailsViewDenormalizer(IReadSideRepositoryWriter<SurveyDetailsView> repositoryWriter)
            : base(repositoryWriter) {}

        public SurveyDetailsView Update(SurveyDetailsView state, IPublishedEvent<NewSurveyStarted> @event)
        {
            return new SurveyDetailsView
            {
                SurveyId = @event.EventSourceId.FormatGuid(),
                Name = @event.Payload.Name
            };
        }

        public SurveyDetailsView Update(SurveyDetailsView state, IPublishedEvent<SupervisorRegistered> @event)
        {
            var supervisor = new SupervisorView()
            {
                Login = @event.Payload.Login
            };

            state.Supervisors.Add(supervisor);

            return state;
        }
    }
}