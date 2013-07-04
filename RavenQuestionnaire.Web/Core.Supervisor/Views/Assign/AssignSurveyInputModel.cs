using System;

namespace Core.Supervisor.Views.Assign
{
    public class AssignSurveyInputModel
    {
        public AssignSurveyInputModel(Guid id, Guid viewerId)
        {
            this.CompleteQuestionnaireId = id;
            this.ViewerId = viewerId;
        }

        public Guid ViewerId { get; set; }
        
        public Guid CompleteQuestionnaireId { get; private set; }
    }
}