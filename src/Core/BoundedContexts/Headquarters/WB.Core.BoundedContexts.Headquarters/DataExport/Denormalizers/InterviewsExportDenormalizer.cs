using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers
{
    public class InterviewsExportDenormalizer : BaseDenormalizer,
        IEventHandler<InterviewStatusChanged>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewHardDeleted>
    {
        private readonly IReadSideRepositoryWriter<InterviewDataExportRecord> exportRecords;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewEntity> interviewRepository;
        private readonly IInterviewFactory interviewFactory;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewReferences;
        private readonly IExportViewFactory exportViewFactory;

        private readonly IReadOnlyList<InterviewStatus> generateExportForStatuses = new List<InterviewStatus>
        {
            InterviewStatus.ApprovedByHeadquarters,
            InterviewStatus.ApprovedBySupervisor,
            InterviewStatus.Completed,
            InterviewStatus.InterviewerAssigned,
            InterviewStatus.Restored
        };

        public InterviewsExportDenormalizer(IInterviewFactory interviewFactory,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewReferences,
            IExportViewFactory exportViewFactory, 
            IReadSideRepositoryWriter<InterviewDataExportRecord> exportRecords,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage,
            IQueryableReadSideRepositoryReader<InterviewEntity> interviewRepository)
        {
            this.interviewFactory = interviewFactory;
            this.interviewReferences = interviewReferences;
            this.exportViewFactory = exportViewFactory;
            this.exportRecords = exportRecords;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.interviewRepository = interviewRepository;
        }

        public override object[] Writers => new object[] { this.exportRecords };
        public override object[] Readers => new object[] { this.interviewRepository, this.interviewReferences };

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            if (this.generateExportForStatuses.Contains(evnt.Payload.Status))
            {
                var interviewId = evnt.EventSourceId;

                var questionnaireId = this.interviewReferences.GetQuestionnaireIdentity(interviewId);
                if (questionnaireId != null)
                {
                    var questionnaireExportStructure = this.questionnaireExportStructureStorage
                        .GetQuestionnaireExportStructure(questionnaireId);

                    if (questionnaireExportStructure != null)
                    {
                        var existingRecordKeys = this.exportRecords.GetIdsStartWith(interviewId.ToString());

                        InterviewDataExportView interviewDataExportView =
                            this.exportViewFactory.CreateInterviewDataExportView(questionnaireExportStructure, this.interviewFactory.GetInterviewData(interviewId));

                        var records = interviewDataExportView.GetAsRecords().ToList();

                        foreach (var record in records)
                        {
                            this.exportRecords.Store(record, record.Id);
                        }

                        var recordKeysToDelete = existingRecordKeys.Except(records.Select(x => x.Id)).ToList();

                        foreach (var recordKeyToDelete in recordKeysToDelete)
                        {
                            this.exportRecords.Remove(recordKeyToDelete);
                        }
                    }
                }
            }
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            this.exportRecords.RemoveIfStartsWith(evnt.EventSourceId.ToString());
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            this.exportRecords.RemoveIfStartsWith(evnt.EventSourceId.ToString());
        }
    }
}