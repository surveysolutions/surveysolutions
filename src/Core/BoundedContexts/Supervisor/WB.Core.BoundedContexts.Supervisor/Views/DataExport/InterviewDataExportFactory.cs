using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class InterviewDataExportFactory : IViewFactory<InterviewDataExportInputModel, InterviewDataExportView>
    {
        private readonly IReadSideRepositoryReader<InterviewData> interviewStorage;

        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;

        private readonly IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStorage;

        public InterviewDataExportFactory(IReadSideRepositoryReader<InterviewData> interviewStorage,IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage,
                                          IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStorage)
        {
            this.interviewStorage = interviewStorage;
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.questionnaireExportStorage = questionnaireExportStorage;
        }

        public InterviewDataExportView Load(InterviewDataExportInputModel input)
        {
            var exportStructure = questionnaireExportStorage.GetById(input.TemplateId,
                input.TemplateVersion);
            var exportStructureForLevel = exportStructure.HeaderToLevelMap[input.LevelId ?? input.TemplateId];

            var records = BuildRecordsForHeader(input.TemplateId, input.TemplateVersion,
                                                input.LevelId, exportStructureForLevel);

            var exportedData = new InterviewDataExportView(input.TemplateId, input.TemplateVersion, input.LevelId,
                                                           exportStructureForLevel, records);
            return exportedData;
        }

        public IList<InterviewSummary> GetApprovedInterviews(Guid templateId,
                                                                  long templateVersion, int bulkSize=256)
        {
            var result = new List<InterviewSummary>();
            int returnedEventCount = 0;
            while (true)
            {
                var interviews =
                    this.interviewSummaryStorage.Query(
                        _ =>
                        _.Where(
                            interview =>
                            interview.QuestionnaireId == templateId && interview.QuestionnaireVersion == templateVersion &&
                            interview.Status == InterviewStatus.ApprovedBySupervisor).Skip(returnedEventCount)
                        .Take(bulkSize).ToArray());

                result.AddRange(interviews);

                bool allEventsWereAlreadyReturned = interviews.Length < bulkSize;

                if (allEventsWereAlreadyReturned)
                     break;

                returnedEventCount += bulkSize;
            }
            return result;
        }

        private InterviewDataExportRerord[] BuildRecordsForHeader(Guid templateId,
                                                                  long templateVersion, Guid? levelId, HeaderStructureForLevel header)
        {
            var dataRecords = new List<InterviewDataExportRerord>();

            var interviews =
                GetApprovedInterviews(templateId, templateVersion)
                    .Select(interview => this.interviewStorage.GetById(interview.InterviewId));

            int recordId = 0;

            foreach (var interview in interviews)
            {
                recordId = this.FillDataRecordsWithDataFromInterviewByLevelAndReturnIndexOfNextRecord(dataRecords, interview, levelId, header, recordId);
            }
            
            return dataRecords.ToArray();
        }

        private int FillDataRecordsWithDataFromInterviewByLevelAndReturnIndexOfNextRecord(List<InterviewDataExportRerord> dataRecords,
            InterviewData interview, Guid? levelId, HeaderStructureForLevel header, int recordIndex)
        {
            var interviewDataByLevels = GetLevelsFromInterview(interview, levelId);

            foreach (var dataByLevel in interviewDataByLevels)
            {
                AddDataRecordFromInterviewLevel(dataRecords, dataByLevel, recordIndex, interview.InterviewId, header);
                if (levelId.HasValue) recordIndex++;
            }

            //increase only in case of top level, if I increase record index inside roaster, linked questions data would be broken
            recordIndex = !levelId.HasValue ? recordIndex + 1 : 0;
            return recordIndex;
        }

        private IEnumerable<InterviewLevel> GetLevelsFromInterview(InterviewData interview, Guid? levelId)
        {
            if (!levelId.HasValue)
            {
                return interview.Levels.Values.Where(level => level.ScopeIds.ContainsKey(interview.InterviewId));
            }
            return interview.Levels.Values.Where(level => level.ScopeIds.ContainsKey(levelId.Value));
        }

        private void AddDataRecordFromInterviewLevel(List<InterviewDataExportRerord> dataRecords, InterviewLevel level,
                                                     int recordId,
                                                     Guid interviewId, HeaderStructureForLevel header)
        {
#warning parentid is always null
            var record = new InterviewDataExportRerord(interviewId, recordId, GetParentRecordIndex(level), BuildExportedQuestionsByLevel(level, header));
            dataRecords.Add(record);
        }

        private decimal? GetParentRecordIndex(InterviewLevel level)
        {
            if (level.RosterVector.Length < 2)
                return null;
            return level.RosterVector[level.RosterVector.Length - 2];
        }

        private Dictionary<Guid, ExportedQuestion> BuildExportedQuestionsByLevel(InterviewLevel level,  HeaderStructureForLevel header)
        {
            var answeredQuestions = new Dictionary<Guid,ExportedQuestion>();

            foreach (var question in level.GetAllQuestions())
            {
                var headerItem = header.HeaderItems[question.Id];

                if (headerItem == null)
                    continue;

                var exportedQuestion = new ExportedQuestion(question, headerItem);
                answeredQuestions.Add(question.Id, exportedQuestion);
            }

            return answeredQuestions;
        }
    }
}
