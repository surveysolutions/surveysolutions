using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers
{
    internal class SurveyLineViewDenormalizer : AbstractFunctionalEventHandler<SurveyLineView>,
        IUpdateHandler<SurveyLineView, NewSurveyStarted>
    {
        public SurveyLineViewDenormalizer(IReadSideRepositoryWriter<SurveyLineView> repositoryWriter)
            : base(repositoryWriter) {}

        public SurveyLineView Update(SurveyLineView state, IPublishedEvent<NewSurveyStarted> @event)
        {
            return new SurveyLineView
            {
                SurveyId = @event.EventSourceId.FormatGuid(),
                Name = @event.Payload.Name,
            };
        }
    }
}