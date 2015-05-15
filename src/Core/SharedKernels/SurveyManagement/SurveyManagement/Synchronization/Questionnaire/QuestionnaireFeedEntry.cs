using System;
using WB.Core.SharedKernels.SurveySolutions;

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

        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual QuestionnaireEntryType EntryType { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual string EntryId { get; set; }
    }
}
