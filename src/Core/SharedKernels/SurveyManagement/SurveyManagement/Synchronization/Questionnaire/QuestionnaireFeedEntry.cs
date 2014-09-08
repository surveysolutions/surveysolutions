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
        public QuestionnaireFeedEntry(Guid questionnaireId, long questionnaireVersion, string entryId, QuestionnaireEntryType entryType, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(entryId))
            {
                throw new ArgumentNullException("entryId");
            }

            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.EntryType = entryType;
            this.Timestamp = timestamp;
            this.EntryId = entryId;
        }

        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public QuestionnaireEntryType EntryType { get; set; }
        public DateTime Timestamp { get; set; }
        public string EntryId { get; set; }
    }
}
