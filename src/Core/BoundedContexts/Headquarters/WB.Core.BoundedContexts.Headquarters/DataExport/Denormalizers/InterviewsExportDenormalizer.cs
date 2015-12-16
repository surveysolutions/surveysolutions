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
            IExportViewFactory exportViewFactory)
        {
            this.dataExportRecords = dataExportRecords;
            this.interviewDatas = interviewDatas;
            this.questionnaireExportStructures = questionnaireExportStructures;
            this.interviewReferences = interviewReferences;
            this.exportViewFactory = exportViewFactory;
        }

        public override object[] Writers => new object[] { this.dataExportRecords };
        public override object[] Readers => new object[] { this.interviewDatas, this.questionnaireExportStructures, this.interviewReferences };

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            if (this.GenerateExportForStatuses.Contains(evnt.Payload.Status))
            {
                var interviewReference = this.interviewReferences.GetById(evnt.EventSourceId);

                var questionnaireExportStructure = this.questionnaireExportStructures.AsVersioned().Get(interviewReference.QuestionnaireId.FormatGuid(),
                    interviewReference.QuestionnaireVersion);

                InterviewDataExportView interviewDataExportView =
                    this.exportViewFactory.CreateInterviewDataExportView(questionnaireExportStructure, this.interviewDatas.GetById(evnt.EventSourceId));

                this.dataExportRecords.Store(interviewDataExportView, evnt.EventSourceId);
            }
        }
    }
}