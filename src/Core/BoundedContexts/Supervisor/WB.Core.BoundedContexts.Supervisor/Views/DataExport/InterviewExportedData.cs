using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class InterviewExportedData : IView
    {
        public InterviewExportedData()
        {
            this.InterviewDataByLevels = new InterviewExportedLevel[0];
        }

        public InterviewExportedData(InterviewData interviewData, InterviewStatus status,
            QuestionnaireExportStructure questionnaireExportStructure)
            : this()
        {
            InterviewId = interviewData.InterviewId;
            Status = status;
            QuestionnaireId = interviewData.QuestionnaireId;
            QuestionnaireVersion = interviewData.QuestionnaireVersion;

            var interviewDataByLevels = new List<InterviewExportedLevel>();
            foreach (var scopeId in questionnaireExportStructure.HeaderToLevelMap.Keys)
            {
                var levelsByScope = GetLevelsFromInterview(interviewData, scopeId);
                interviewDataByLevels.AddRange(
                    levelsByScope.Select(
                        interviewLevel =>
                            new InterviewExportedLevel(scopeId, interviewLevel.RosterVector,
                                this.BuildExportedQuestionsByLevel(interviewLevel, questionnaireExportStructure.HeaderToLevelMap[scopeId]))));
            }

            this.InterviewDataByLevels = interviewDataByLevels.ToArray();
        }

        public Guid InterviewId { get; set; }
        public InterviewStatus Status { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public InterviewExportedLevel[] InterviewDataByLevels { get; set; }

        private ExportedQuestion[] BuildExportedQuestionsByLevel(InterviewLevel level, HeaderStructureForLevel header)
        {
            var answeredQuestions = new List<ExportedQuestion>();

            foreach (var question in level.GetAllQuestions())
            {
                var headerItem = header.HeaderItems[question.Id];

                if (headerItem == null)
                    continue;

                var exportedQuestion = new ExportedQuestion(question, headerItem);
                answeredQuestions.Add(exportedQuestion);
            }

            return answeredQuestions.ToArray();
        }

        private IEnumerable<InterviewLevel> GetLevelsFromInterview(InterviewData interview, Guid levelId)
        {
            if (levelId == interview.QuestionnaireId)
                return interview.Levels.Values.Where(level => level.ScopeIds.ContainsKey(interview.InterviewId));
            return interview.Levels.Values.Where(level => level.ScopeIds.ContainsKey(levelId));
        }
    }
}
