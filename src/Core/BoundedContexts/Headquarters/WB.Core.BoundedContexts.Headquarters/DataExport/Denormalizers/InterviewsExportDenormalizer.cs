using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers
{
    public class InterviewsExportDenormalizer : BaseDenormalizer,
        IEventHandler<InterviewStatusChanged>
    {
        private readonly IReadSideKeyValueStorage<InterviewDataExportView> dataExportRecords;
        private readonly IReadSideRepositoryWriter<InterviewDataExportRecord> exportRecords;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDatas;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructures;
        private readonly IReadSideKeyValueStorage<InterviewReferences> interviewReferences;
        private readonly IExportViewFactory exportViewFactory;

        private readonly IReadOnlyList<InterviewStatus> GenerateExportForStatuses = new List<InterviewStatus>
        {
            InterviewStatus.ApprovedByHeadquarters,
            InterviewStatus.ApprovedBySupervisor,
            InterviewStatus.Completed,
            InterviewStatus.Created,
            InterviewStatus.SupervisorAssigned
        };

        public InterviewsExportDenormalizer(IReadSideKeyValueStorage<InterviewDataExportView> dataExportRecords,
            IReadSideKeyValueStorage<InterviewData> interviewDatas,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructures,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferences,
            IExportViewFactory exportViewFactory, IReadSideRepositoryWriter<InterviewDataExportRecord> exportRecords)
        {
            this.dataExportRecords = dataExportRecords;
            this.interviewDatas = interviewDatas;
            this.questionnaireExportStructures = questionnaireExportStructures;
            this.interviewReferences = interviewReferences;
            this.exportViewFactory = exportViewFactory;
            this.exportRecords = exportRecords;
        }

        public override object[] Writers => new object[] { this.dataExportRecords };
        public override object[] Readers => new object[] { this.interviewDatas, this.questionnaireExportStructures, this.interviewReferences };

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            if (this.GenerateExportForStatuses.Contains(evnt.Payload.Status))
            {
                var interviewId = evnt.EventSourceId;

                var interviewReference = this.interviewReferences.GetById(interviewId);

                var questionnaireExportStructure = this.questionnaireExportStructures.AsVersioned().Get(interviewReference.QuestionnaireId.FormatGuid(),
                    interviewReference.QuestionnaireVersion);

                InterviewDataExportView interviewDataExportView =
                    this.exportViewFactory.CreateInterviewDataExportView(questionnaireExportStructure, this.interviewDatas.GetById(interviewId));

                var records = interviewDataExportView.GetAsRecords().ToList();

                this.exportRecords.BulkStore(Enumerable
                    .Zip(records, records.Select((record, index) => GenerateRecordId(interviewId, index)), Tuple.Create)
                    .ToList());

                //this.dataExportRecords.Store(interviewDataExportView, interviewId);
            }
        }

        private static string GenerateRecordId(Guid interviewId, int index) => $"{interviewId}${index}";
    }
}