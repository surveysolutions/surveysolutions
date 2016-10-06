using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    public class ReplaceTextsCommand : QuestionnaireCommand
    {
        public ReplaceTextsCommand(Guid questionnaireId, Guid responsibleId, string searchFor, string replaceWith)
            : base(questionnaireId, responsibleId)
        {
            this.SearchFor = searchFor;
            this.ReplaceWith = replaceWith;
        }

        public string SearchFor { get; }
        public string ReplaceWith { get; }
    }
}