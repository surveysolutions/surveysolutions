using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Eventing.ServiceModel.Bus;
using Quartz.Util;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class WebInterviewFilteredOptionsRefreshDenormalizer :
        BaseDenormalizer,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<TextListQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<YesNoQuestionAnswered>,
        IEventHandler<AnswersRemoved>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<PictureQuestionAnswered>,
        IEventHandler<QRBarcodeQuestionAnswered>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>
    {
        public override object[] Writers => new object[0];

        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public WebInterviewFilteredOptionsRefreshDenormalizer(IWebInterviewNotificationService webInterviewNotificationService,
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireRepository)
        {
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.questionnaireRepository = questionnaireRepository;
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<TextListQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<YesNoQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<PictureQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
            => RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);

        private void RefreshEntitiesWithFilteredOptions(Guid interviewId)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());
            var document = this.questionnaireRepository.GetQuestionnaireDocument(interview.QuestionnaireIdentity);

            var entityIds = document.Find<IComposite>(IsSupportFilterOptionCondition)
                .Select(e => e.PublicKey).ToHashSet();

            foreach (var entityId in entityIds)
            {
                var identities = interview.GetAllIdentitiesForEntityId(entityId).ToArray();
                this.webInterviewNotificationService.RefreshEntities(interviewId, identities);
            }
        }

        private bool IsSupportFilterOptionCondition(IComposite documentEntity)
        {
            var question = documentEntity as IQuestion;
            if (question != null && !question.Properties.OptionsFilterExpression.IsNullOrWhiteSpace())
                return true;

            return false;
        }
    }
}