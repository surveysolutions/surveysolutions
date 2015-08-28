using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class InterviewExportedDataDenormalizer :
        BaseDenormalizer, IAtomicEventHandler,
        IEventHandler<InterviewApprovedByHQ>,
        IEventHandler<SupervisorAssigned>,
        IEventHandler<InterviewerAssigned>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<InterviewApproved>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewHardDeleted>,
        IEventHandler<InterviewRejected>,
        IEventHandler<InterviewRejectedByHQ>,
        IEventHandler<InterviewRestored>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;

        private readonly IDataExportRepositoryWriter dataExportWriter;

        public InterviewExportedDataDenormalizer(IDataExportRepositoryWriter dataExportWriter,
            IReadSideRepositoryWriter<UserDocument> userDocumentWriter,
            IReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage)
        {
            this.dataExportWriter = dataExportWriter;
            this.users = userDocumentWriter;
            this.interviewSummaryStorage = interviewSummaryStorage;
        }

        public override object[] Writers
        {
            get { return new object[] {dataExportWriter}; }
        }

        public void CleanWritersByEventSource(Guid eventSourceId)
        {
            this.dataExportWriter.DeleteInterview(eventSourceId);
        }

        public override object[] Readers
        {
            get { return new object[] {users, interviewSummaryStorage}; }
        }

        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            this.dataExportWriter.AddExportedDataByInterviewWithAction(evnt.EventSourceId,
                InterviewExportedAction.ApprovedByHeadquarter);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            this.dataExportWriter.AddExportedDataByInterviewWithAction(evnt.EventSourceId,
                InterviewExportedAction.SupervisorAssigned);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            this.dataExportWriter.AddExportedDataByInterviewWithAction(evnt.EventSourceId,
                InterviewExportedAction.Completed);
        }

        public void Handle(IPublishedEvent<InterviewApproved> evnt)
        {
            this.dataExportWriter.AddExportedDataByInterviewWithAction(evnt.EventSourceId,
                InterviewExportedAction.ApprovedBySupervisor);
        }

        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
            this.dataExportWriter.AddExportedDataByInterviewWithAction(evnt.EventSourceId,
                InterviewExportedAction.RejectedBySupervisor);
        }

        public void Handle(IPublishedEvent<InterviewRestored> evnt)
        {
            this.dataExportWriter.AddExportedDataByInterviewWithAction(evnt.EventSourceId,
                InterviewExportedAction.Restored);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            this.dataExportWriter.AddExportedDataByInterviewWithAction(evnt.EventSourceId,
                InterviewExportedAction.InterviewerAssigned);
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            this.dataExportWriter.AddExportedDataByInterviewWithAction(evnt.EventSourceId,
                InterviewExportedAction.Restarted);
        }

        public void Handle(IPublishedEvent<InterviewRejectedByHQ> evnt)
        {

            this.dataExportWriter.AddExportedDataByInterviewWithAction(evnt.EventSourceId,
                InterviewExportedAction.RejectedByHeadquarter);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            this.dataExportWriter.DeleteInterview(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            this.dataExportWriter.DeleteInterview(evnt.EventSourceId);
        }
    }
}
