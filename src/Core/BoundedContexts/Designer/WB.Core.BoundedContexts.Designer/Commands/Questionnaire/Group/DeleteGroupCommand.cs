using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    public class DeleteGroupCommand : GroupCommand
    {
        public DeleteGroupCommand(Guid questionnaireId, Guid groupId, Guid responsibleId)
            : base(questionnaireId, groupId, responsibleId) {}
    }
}