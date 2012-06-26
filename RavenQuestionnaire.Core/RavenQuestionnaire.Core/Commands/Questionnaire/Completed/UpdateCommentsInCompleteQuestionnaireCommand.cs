using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    public class UpdateCommentsInCompleteQuestionnaireCommand:ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        public Guid QuestionPublickey { get; private set; }
        public Guid? Propagationkey { get; private set; }
        public string Comments { get; set; }
        public UserLight Executor{ get; set; }
        
        public UpdateCommentsInCompleteQuestionnaireCommand(string completeQuestionnaireId, CompleteQuestionView question,
                                        Guid? propagationkey, UserLight executor)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
            this.Executor = executor;
            this.QuestionPublickey = question.PublicKey;
            this.Propagationkey = propagationkey;
            this.Comments = question.Comments;
        }
    }
}