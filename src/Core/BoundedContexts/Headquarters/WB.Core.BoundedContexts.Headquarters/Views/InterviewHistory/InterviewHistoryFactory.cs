﻿using System;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory
{
    internal class InterviewHistoryFactory : IInterviewHistoryFactory
    {
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IUserViewFactory userReader;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IEventStore eventStore;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ILogger logger;

        public InterviewHistoryFactory(
            IEventStore eventStore, 
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            IUserViewFactory userReader,
            ILogger logger, InterviewDataExportSettings interviewDataExportSettings, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage)
        {
            this.eventStore = eventStore;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.logger = logger;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        public InterviewHistoryView Load(Guid interviewId)
        {
            return this.RestoreInterviewHistory(interviewId);
        }

        private InterviewHistoryView RestoreInterviewHistory(Guid interviewId)
        {
            var interviewHistoryReader = new InMemoryReadSideRepositoryAccessor<InterviewHistoryView>();
            var interviewHistoryDenormalizer =
                new InterviewParaDataEventHandler(interviewHistoryReader, this.interviewSummaryReader, this.userReader, this.interviewDataExportSettings, this.questionnaireExportStructureStorage);

            var events = this.eventStore.Read(interviewId, 0);
            foreach (var @event in events)
            {
                interviewHistoryDenormalizer.Handle(@event);
            }
            return interviewHistoryReader.GetById(interviewId);
        }
    }
}
