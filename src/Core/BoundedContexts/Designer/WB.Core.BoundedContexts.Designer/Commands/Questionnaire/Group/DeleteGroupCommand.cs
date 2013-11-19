﻿using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "DeleteGroup")]
    public class DeleteGroupCommand : GroupCommand
    {
        public DeleteGroupCommand(Guid questionnaireId, Guid groupId, Guid responsibleId)
            : base(questionnaireId, groupId, responsibleId) {}
    }
}