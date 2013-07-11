using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UpdateQuestionnaire")]
    public class UpdateQuestionnaireCommand : QuestionnaireCommand
    {
        public UpdateQuestionnaireCommand(Guid questionnaireId, string title, bool isPublic)
            : base(questionnaireId)
        {
            this.Title = title;
            this.IsPublic = isPublic;
        }

        public string Title { get; set; }

        public bool IsPublic { get; set; }
    }
}