using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewDeleteGroup")]
    public class DeleteGroupCommand : GroupCommand
    {
        public DeleteGroupCommand(Guid questionnaireId, Guid groupId)
            : base(questionnaireId, groupId) {}
    }
}