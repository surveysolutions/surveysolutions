using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Statistics;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerView
    {
        public InterviewerView()
        {
            
        }
        public InterviewerView(IEnumerable<CompleteQuestionnaireStatisticView> questionnaires)
        {
            this.Questionnaires = questionnaires;
        }
        public IEnumerable<CompleteQuestionnaireStatisticView> Questionnaires { get; set; }
    }
}
