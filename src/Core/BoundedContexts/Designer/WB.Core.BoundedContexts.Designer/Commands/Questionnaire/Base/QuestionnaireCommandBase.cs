using System;
using Ncqrs.Commanding;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public class QuestionnaireCommandBase : CommandBase
    {
        public QuestionnaireCommandBase(Guid responsibleId)
        {
            this.ResponsibleId = responsibleId;
        }

        public Guid ResponsibleId { get; set; }
    }
}