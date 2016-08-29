using System;
using Ncqrs.Commanding;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public class QuestionnaireCommandBase : CommandBase
    {
        public QuestionnaireCommandBase(Guid responsibleId, bool isResponsibleAdmin)
        {
            this.IsResponsibleAdmin = isResponsibleAdmin;
            this.ResponsibleId = responsibleId;
        }

        public Guid ResponsibleId { get; set; }
        public bool IsResponsibleAdmin { get; set; }
    }
}