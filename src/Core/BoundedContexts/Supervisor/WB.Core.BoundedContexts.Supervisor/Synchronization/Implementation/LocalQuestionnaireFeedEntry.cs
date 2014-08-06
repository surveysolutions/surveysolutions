using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    public class LocalQuestionnaireFeedEntry : QuestionnaireFeedEntry
    {
        public bool Processed { get; set; }

        public bool ProcessedWithError { get; set; }

        public Uri QuestionnaireUri { get; set; }
    }
}
