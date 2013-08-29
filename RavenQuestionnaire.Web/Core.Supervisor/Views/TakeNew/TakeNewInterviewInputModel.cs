using System;

namespace Core.Supervisor.Views.TakeNew
{
    public class TakeNewInterviewInputModel
    {
        public TakeNewInterviewInputModel(Guid questionnaireId, Guid viewerId)
        {
            this.QuestionnaireId = questionnaireId;
            this.ViewerId = viewerId;
        }

        public Guid ViewerId { get; set; }

        public Guid QuestionnaireId { get; private set; }
    }
}