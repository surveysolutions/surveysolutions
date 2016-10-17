using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers
{
    public class InterviewsExportDenormalizer : BaseDenormalizer,
        IEventHandler<InterviewStatusChanged>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewHardDeleted>
    {
        private readonly IReadSideRepositoryWriter<InterviewDataExportRecord> exportRecords;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDatas;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IReadSideKeyValueStorage<InterviewReferences> interviewReferences;
        private readonly IExportViewFactory exportViewFactory;

        private readonly IReadOnlyList<InterviewStatus> GenerateExportForStatuses = new List<InterviewStatus>
        {
            InterviewStatus.ApprovedByHeadquarters,
            InterviewStatus.ApprovedBySupervisor,
            InterviewStatus.Completed,
            InterviewStatus.InterviewerAssigned,
            InterviewStatus.Restored
        };

        public InterviewsExportDenormalizer(IReadSideKeyValueStorage<InterviewData> interviewDatas,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferences,
            IExportViewFactory exportViewFactory, 
            IReadSideRepositoryWriter<InterviewDataExportRecord> exportRecords,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage)
        {
            this.interviewDatas = interviewDatas;
            this.interviewReferences = interviewReferences;
            this.exportViewFactory = exportViewFactory;
            this.exportRecords = exportRecords;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        public override object[] Writers => new object[] { this.exportRecords };
        public override object[] Readers => new object[] { this.interviewDatas, this.interviewReferences };

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            if (this.GenerateExportForStatuses.Contains(evnt.Payload.Status))
            {
                var interviewId = evnt.EventSourceId;

                this.exportRecords.RemoveIfStartsWith(interviewId.ToString());

                var interviewReference = this.interviewReferences.GetById(interviewId);

                var questionnaireExportStructure =
                    this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(
                        new QuestionnaireIdentity(interviewReference.QuestionnaireId,
                            interviewReference.QuestionnaireVersion));

                if (questionnaireExportStructure != null)
                {
                    InterviewDataExportView interviewDataExportView =
                        this.exportViewFactory.CreateInterviewDataExportView(questionnaireExportStructure,
                            this.interviewDatas.GetById(interviewId));

                    var records = interviewDataExportView.GetAsRecords().ToList();

                    foreach (var record in records)
                    {
                        this.exportRecords.Store(record, record.Id);
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