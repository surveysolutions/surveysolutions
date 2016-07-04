using System;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Views.Preloading
{
    public class QuestionnairePreloadingDataItem
    {
        public QuestionnairePreloadingDataItem(Guid questionnaireId, long version, string title, QuestionDescription[] questions)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Title = title;
            this.Questions = questions;
        }

        public Guid QuestionnaireId { get; private set; }

        public long Version { get; private set; }

        public string Title { get; private set; }

        public QuestionDescription[] Questions { get; private set; }
    }
}
