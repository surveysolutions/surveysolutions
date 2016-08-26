using System;
using Ncqrs.Commanding;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public class QuestionnaireCommandBase : CommandBase
    {
        public QuestionnaireCommandBase(Guid responsibleId, bool hasResponsibleAdminRights)
        {
            this.HasResponsibleAdminRights = hasResponsibleAdminRights;
            this.ResponsibleId = responsibleId;
        }

        public Guid ResponsibleId { get; set; }
        public bool HasResponsibleAdminRights { get; set; }
    }
}