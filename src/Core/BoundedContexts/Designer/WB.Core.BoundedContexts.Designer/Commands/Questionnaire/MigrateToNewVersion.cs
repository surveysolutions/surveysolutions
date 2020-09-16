using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    public class MigrateToNewVersion : QuestionnaireCommand
    {
        public MigrateToNewVersion(Guid questionnaireId, Guid responsibleId) : base(questionnaireId, responsibleId)
        {
        }
    }
}