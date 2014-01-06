using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewExportedDataEventHandler : AbstractFunctionalEventHandler<InterviewExportedData>,
        ICreateHandler<InterviewExportedData, InterviewApproved>
    {
        private readonly IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter;

        public InterviewExportedDataEventHandler(IReadSideRepositoryWriter<InterviewExportedData> readSideRepositoryWriter,
            IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter,
            IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter)
            : base(readSideRepositoryWriter)
        {
            this.interviewDataWriter = interviewDataWriter;
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
        }

        public override Type[] UsesViews
        {
            get { return new Type[] { typeof (InterviewData) }; }
        }

        public InterviewExportedData Create(IPublishedEvent<InterviewApproved> evnt)
        {
            var interview = interviewDataWriter.GetById(evnt.EventSourceId);

            var exportStructure = questionnaireExportStructureWriter.GetById(interview.Document.QuestionnaireId,
                interview.Document.QuestionnaireVersion);

            return new InterviewExportedData(interview.Document, InterviewStatus.ApprovedBySupervisor, exportStructure);
        }
    }
}
