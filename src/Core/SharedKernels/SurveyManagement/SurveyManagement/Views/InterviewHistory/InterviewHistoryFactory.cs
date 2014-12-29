﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    internal class InterviewHistoryFactory : IInterviewHistoryFactory
    {
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IReadSideRepositoryWriter<UserDocument> userReader;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireReader;
        private readonly IEventStore eventStore;
        private readonly ILogger logger;

        public InterviewHistoryFactory(IEventStore eventStore, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            IReadSideRepositoryWriter<UserDocument> userReader,
            IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireReader, ILogger logger)
        {
            this.eventStore = eventStore;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.questionnaireReader = questionnaireReader;
            this.logger = logger;
        }

        public InterviewHistoryView Load(Guid interviewId)
        {
            return this.RestoreInterviewHistory(interviewId);
        }

        private InterviewHistoryView RestoreInterviewHistory(Guid interviewId)
        {
            var interviewHistoryReader = new InMemoryReadSideRepositoryAccessor<InterviewHistoryView>();
            var interviewHistoryDenormalizer =
                new InterviewHistoryDenormalizer(interviewHistoryReader, interviewSummaryReader, userReader, questionnaireReader);

            var events = this.eventStore.ReadFrom(interviewId, 0, long.MaxValue);
            foreach (var @event in events)
            {
                this.PublishToHandlers(@event, interviewHistoryDenormalizer);
            }
            return interviewHistoryReader.GetById(interviewId);
        }

        private void PublishToHandlers(IPublishableEvent eventMessage, 
            InterviewHistoryDenormalizer interviewHistoryDenormalizer)
        {

            var publishedEventClosedType = typeof(IPublishableEvent);
            var handleMethod = typeof(InterviewHistoryDenormalizer).GetMethod("Handle", new[] { publishedEventClosedType });

            if(handleMethod==null)
                return;

            var occurredExceptions = new List<Exception>();
            
            try
            {
                handleMethod.Invoke(interviewHistoryDenormalizer, new object[] { eventMessage });
            }
            catch (Exception exception)
            {
                logger.Error(exception.Message, exception);
                occurredExceptions.Add(exception);
            }
        }
    }
}
