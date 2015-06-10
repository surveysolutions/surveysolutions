using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireStateTacker : IView
    {
        public QuestionnaireStateTacker()
        {
            QuestionsState = new Dictionary<Guid, string>();
            GroupsState = new Dictionary<Guid, string>();
            StaticTextState = new Dictionary<Guid, string>();
        }

        public Dictionary<Guid, string> QuestionsState { get; set; }
        public Dictionary<Guid, string> GroupsState { get; set; }
        public Dictionary<Guid, string> StaticTextState { get; set; }
        public Guid CreatedBy { get; set; }
    }
}