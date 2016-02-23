using System;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    internal class InterviewHistoryFactory : IInterviewHistoryFactory
    {
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IReadSideRepositoryWriter<UserDocument> userReader;

        private readonly IQuestionnaireProjectionsRepository questionnaireProjectionsRepository;
        private readonly IEventStore eventStore;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ILogger logger;

        public InterviewHistoryFactory(
            IEventStore eventStore, 
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            IReadSideRepositoryWriter<UserDocument> userReader,
            ILogger logger, InterviewDataExportSettings interviewDataExportSettings, 
            IQuestionnaireProjectionsRepository questionnaireProjectionsRepository)
        {
            this.eventStore = eventStore;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.logger = logger;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.questionnaireProjectionsRepository = questionnaireProjectionsRepository;
        }

        public InterviewHistoryView Load(Guid interviewId)
        {
            return this.RestoreInterviewHistory(interviewId);
        }

        private InterviewHistoryView RestoreInterviewHistory(Guid interviewId)
        {
            var interviewHistoryReader = new InMemoryReadSideRepositoryAccessor<InterviewHistoryView>();
            var interviewHistoryDenormalizer =
                new InterviewParaDataEventHandler(interviewHistoryReader, interviewSummaryReader, userReader, interviewDataExportSettings, questionnaireProjectionsRepository);

            var events = this.eventStore.ReadFrom(interviewId, 0, int.MaxValue);
            foreach (var @event in events)
            {
                interviewHistoryDenormalizer.Handle(@event);
            }
            return interviewHistoryReader.GetById(interviewId);
        }
    }
}
