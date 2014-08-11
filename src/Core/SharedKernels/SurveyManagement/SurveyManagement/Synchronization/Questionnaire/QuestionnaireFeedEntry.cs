using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire
{
    public class QuestionnaireFeedEntry : IReadSideRepositoryEntity
    {
        public QuestionnaireFeedEntry() { }
        public QuestionnaireFeedEntry(Guid questionnaireId, long questionnaireVersion, string entryId, bool allowCensusMode, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(entryId))
            {
                throw new ArgumentNullException("entryId");
            }

            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.AllowCensusMode = allowCensusMode;
            this.Timestamp = timestamp;
            this.EntryId = entryId;
        }

        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public bool AllowCensusMode { get; set; }
        public DateTime Timestamp { get; set; }
        public string EntryId { get; set; }
    }
}
