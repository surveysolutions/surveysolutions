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
        private readonly IQueryableReadSideRepositoryReader<InterviewExportedData> interviewExportedDataStorage;

        private readonly IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStorage;

        public InterviewDataExportFactory(
                                          IQueryableReadSideRepositoryReader<InterviewExportedData> interviewExportedDataStorage,
                                          IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStorage)
        {
            this.interviewExportedDataStorage = interviewExportedDataStorage;
            this.questionnaireExportStorage = questionnaireExportStorage;
        }

        public InterviewDataExportView Load(InterviewDataExportInputModel input)
        {
            var exportStructure = questionnaireExportStorage.GetById(input.TemplateId,
                input.TemplateVersion);
            var exportStructureForLevel = exportStructure.HeaderToLevelMap[input.LevelId ?? input.TemplateId];

            var records = BuildRecordsForHeader(input.TemplateId, input.TemplateVersion,
                                                input.LevelId);

            var exportedData = new InterviewDataExportView(input.TemplateId, input.TemplateVersion, input.LevelId,
                                                           exportStructureForLevel, records);
            return exportedData;
        }

        public IList<InterviewExportedData> GetApprovedInterviews(Guid templateId,
                                                                  long templateVersion, int bulkSize=256)
        {
            var result = new List<InterviewExportedData>();
            int returnedEventCount = 0;
            while (true)
            {
                var interviews =
                    this.interviewExportedDataStorage.Query(
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

        private InterviewDataExportRecord[] BuildRecordsForHeader(Guid templateId,
                                                                  long templateVersion, Guid? levelId)
        {
            var dataRecords = new List<InterviewDataExportRecord>();

            var interviews =
                GetApprovedInterviews(templateId, templateVersion);

            int recordId = 0;

            foreach (var interview in interviews)
            {
                recordId = this.FillDataRecordsWithDataFromInterviewByLevelAndReturnIndexOfNextRecord(dataRecords, interview, levelId, recordId);
            }
            
            return dataRecords.ToArray();
        }

        private int FillDataRecordsWithDataFromInterviewByLevelAndReturnIndexOfNextRecord(List<InterviewDataExportRecord> dataRecords,
            InterviewExportedData interview, Guid? levelId, int recordIndex)
        {
            var interviewDataByLevels = GetLevelsFromInterview(interview, levelId);

            foreach (var dataByLevel in interviewDataByLevels)
            {
                dataRecords.Add(CreateDataRecordFromInterviewLevel(dataByLevel, recordIndex, interview.InterviewId));
                if (levelId.HasValue) recordIndex++;
            }

            //increase only in case of top level, if I increase record index inside roaster, linked questions data would be broken
            recordIndex = !levelId.HasValue ? recordIndex + 1 : 0;
            return recordIndex;
        }

        private IEnumerable<ExportedQuestion[]> GetLevelsFromInterview(InterviewExportedData interview, Guid? levelId)
        {
            if (!levelId.HasValue)
            {
                return interview.InterviewDataByLevels.Where(level => level.RosterVector.Length == 0).Select(level => level.Questions);
            }
            return interview.InterviewDataByLevels.Where(level => level.ScopeId == levelId.Value).Select(level => level.Questions);
        }

        private InterviewDataExportRecord CreateDataRecordFromInterviewLevel(ExportedQuestion[] exportedQuestions,
                                                     int recordId,
                                                     Guid interviewId)
        {
#warning parentid is always null
            return new InterviewDataExportRecord(interviewId, recordId, null, exportedQuestions);
        }
    }
}
