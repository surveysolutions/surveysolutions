using Main.Core.Commands.Questionnaire.Base;

namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewDeleteGroup")]
    public class DeleteGroupCommand : GroupCommand
    {
        public DeleteGroupCommand(Guid questionnaireId, Guid groupId)
            : base(questionnaireId, groupId) {}
    }
}