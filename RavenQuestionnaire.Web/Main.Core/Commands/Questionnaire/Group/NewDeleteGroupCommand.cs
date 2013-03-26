namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewDeleteGroup")]
    public class NewDeleteGroupCommand : GroupCommand
    {
        public NewDeleteGroupCommand(Guid questionnaireId, Guid groupId)
            : base(questionnaireId, groupId) {}
    }
}