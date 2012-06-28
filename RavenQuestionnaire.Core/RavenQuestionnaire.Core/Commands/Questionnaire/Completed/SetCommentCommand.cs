using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "SetComment")]
    public class SetCommentCommand : CommandBase
    {
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }
        public Guid QuestionPublickey { get; private set; }
        public string Comments { get; set; }
        public Guid? PropogationPublicKey { get; set; }
        public SetCommentCommand(Guid completeQuestionnaireId, CompleteQuestionView question,
                                        Guid? propogationPublicKey)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
         //   this.Executor = executor;
            this.QuestionPublickey = question.PublicKey;
            this.PropogationPublicKey = propogationPublicKey;
            this.Comments = question.Comments;
        }
    }
}
