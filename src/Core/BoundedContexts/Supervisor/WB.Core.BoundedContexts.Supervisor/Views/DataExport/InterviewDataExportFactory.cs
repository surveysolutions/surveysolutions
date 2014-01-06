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

            var approvedInterviews = GetApprovedInterviews(input.TemplateId,
                input.TemplateVersion);

            return new InterviewDataExportView(input.TemplateId, input.TemplateVersion,
                exportStructure.HeaderToLevelMap.Values.Select(
                    exportStructureForLevel =>
                        new InterviewDataExportLevelView(exportStructureForLevel.LevelId, exportStructureForLevel,
                            this.BuildRecordsForHeader(approvedInterviews, exportStructureForLevel.LevelId))).ToArray());
        }

        private InterviewDataExportRecord[] BuildRecordsForHeader(IEnumerable<InterviewExportedData> interviews, Guid levelId)
        {
            var dataRecords = new List<InterviewDataExportRecord>();

            int recordId = 0;

            foreach (var interview in interviews)
            {
                var interviewDataByLevels = this.GetLevelsFromInterview(interview, levelId);

                foreach (var dataByLevel in interviewDataByLevels)
                {
#warning parentid is always null
                    dataRecords.Add(new InterviewDataExportRecord(interview.InterviewId, recordId, null, dataByLevel));
                    if (levelId != interview.QuestionnaireId) recordId++;
                }

                //increase only in case of top level, if I increase record index inside roaster, linked questions data would be broken
                recordId = levelId == interview.QuestionnaireId ? recordId + 1 : 0;
            }

            return dataRecords.ToArray();
        }

        private IEnumerable<ExportedQuestion[]> GetLevelsFromInterview(InterviewExportedData interview, Guid levelId)
        {
            if (levelId == interview.QuestionnaireId)
            {
                return interview.InterviewDataByLevels.Where(level => level.RosterVector.Length == 0).Select(level => level.Questions);
            }
            return interview.InterviewDataByLevels.Where(level => level.ScopeId == levelId).Select(level => level.Questions);
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
    }
}
