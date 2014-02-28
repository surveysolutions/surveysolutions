using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewExportedDataDenormalizer : IEventHandler<InterviewApproved>, IEventHandler
    {
        private readonly IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter;
        private readonly IDataExportService dataExportService;

        public InterviewExportedDataDenormalizer(IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter, IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter, IDataExportService dataExportService)
        {
            this.interviewDataWriter = interviewDataWriter;
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.dataExportService = dataExportService;
        }

        public string Name { get { return this.GetType().Name; } }

        public Type[] UsesViews
        {
            get { return new Type[] { typeof(InterviewData), typeof(QuestionnaireExportStructure) }; }
        }

        public Type[] BuildsViews { get { return new Type[] { typeof(InterviewDataExportView) }; } }

        public void Handle(IPublishedEvent<InterviewApproved> evnt)
        {
            var interview = interviewDataWriter.GetById(evnt.EventSourceId);

            var exportStructure = questionnaireExportStructureWriter.GetById(interview.Document.QuestionnaireId,
                interview.Document.QuestionnaireVersion);

            var interviewDataExportView= new InterviewDataExportView(interview.Document.QuestionnaireId, interview.Document.QuestionnaireVersion,
             exportStructure.HeaderToLevelMap.Values.Select(
                 exportStructureForLevel =>
                     new InterviewDataExportLevelView(exportStructureForLevel.LevelId, exportStructureForLevel.LevelName,
                         this.BuildRecordsForHeader(interview.Document, exportStructureForLevel))).ToArray());
            dataExportService.AddExportedDataByInterview(interviewDataExportView);
        }

        private InterviewDataExportRecord[] BuildRecordsForHeader(InterviewData interview, HeaderStructureForLevel headerStructureForLevel)
        {
            var dataRecords = new List<InterviewDataExportRecord>();

            var interviewDataByLevels = this.GetLevelsFromInterview(interview, headerStructureForLevel.LevelId);

            foreach (var dataByLevel in interviewDataByLevels)
            {
                decimal recordId = dataByLevel.RosterVector.Length == 0 ? 0 : dataByLevel.RosterVector.Last();

#warning parentid is always null
                dataRecords.Add(new InterviewDataExportRecord(interview.InterviewId, recordId, null,
                    GetQuestionsFroExport(dataByLevel.GetAllQuestions(), headerStructureForLevel)));
            }

            return dataRecords.ToArray();
        }

        private ExportedQuestion[] GetQuestionsFroExport(IEnumerable<InterviewQuestion> availableQuestions, HeaderStructureForLevel headerStructureForLevel)
        {
            var result = new List<ExportedQuestion>();
            foreach (var headerItem in headerStructureForLevel.HeaderItems.Values)
            {
                var question = availableQuestions.FirstOrDefault(q => q.Id == headerItem.PublicKey);
                ExportedQuestion exportedQuestion = null;
                if (question == null)
                {
                    var ansnwers = new List<string>();
                    for (int i = 0; i < headerItem.ColumnNames.Count(); i++)
                    {
                        ansnwers.Add(string.Empty);
                    }
                    exportedQuestion = new ExportedQuestion(headerItem.PublicKey, ansnwers.ToArray());
                }
                else
                {
                    exportedQuestion = new ExportedQuestion(question, headerItem);
                }

                result.Add(exportedQuestion);
            }
            return result.ToArray();
        }

        private IEnumerable<InterviewLevel> GetLevelsFromInterview(InterviewData interview, Guid levelId)
        {
            if (levelId == interview.QuestionnaireId)
                return interview.Levels.Values.Where(level => level.ScopeIds.ContainsKey(interview.InterviewId));
            return interview.Levels.Values.Where(level => level.ScopeIds.ContainsKey(levelId));
        }
    }
}
