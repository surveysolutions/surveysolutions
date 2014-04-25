﻿using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "CloneGroup")]
    public class CloneGroupCommand : GroupCommand
    {
        public CloneGroupCommand(Guid questionnaireId, Guid groupId, Guid responsibleId, Guid sourceGroupId,
            int targetIndex)
            : base(questionnaireId: questionnaireId, groupId: groupId, responsibleId: responsibleId)
        {
            this.SourceGroupId = sourceGroupId;
            this.TargetIndex = targetIndex;
        }

        public Guid SourceGroupId { get; private set; }
        public int TargetIndex { get; private set; }
    }
}