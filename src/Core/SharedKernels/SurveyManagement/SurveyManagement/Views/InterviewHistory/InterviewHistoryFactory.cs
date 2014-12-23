using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    public class InterviewHistoryFactory : IViewFactory<InterviewHistoryInputModel, InterviewHistoryView>
    {
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IReadSideRepositoryWriter<UserDocument> userReader;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireReader;
        private readonly IEventStore eventStore;
        public InterviewHistoryFactory(IEventStore eventStore, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader, IReadSideRepositoryWriter<UserDocument> userReader, IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireReader)
        {
            this.eventStore = eventStore;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.questionnaireReader = questionnaireReader;
        }

        public InterviewHistoryView Load(InterviewHistoryInputModel input)
        {
            return this.RestoreInterviewHistory(input);
        }

        private InterviewHistoryView RestoreInterviewHistory(InterviewHistoryInputModel input)
        {
            var interviewHistoryReader = new InMemoryReadSideRepositoryAccessor<InterviewHistoryView>();
            var interviewHistoryDenormalizer =
                new InterviewHistoryDenormalizer(interviewHistoryReader, interviewSummaryReader, userReader, questionnaireReader);

            var events = this.eventStore.ReadFrom(input.InterviewId, 0, long.MaxValue);
            foreach (var @event in events)
            {
                this.PublishToHandlers(@event, interviewHistoryDenormalizer);
            }
            return interviewHistoryReader.GetById(input.InterviewId);
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
                occurredExceptions.Add(exception);
            }
        }
    }
}
