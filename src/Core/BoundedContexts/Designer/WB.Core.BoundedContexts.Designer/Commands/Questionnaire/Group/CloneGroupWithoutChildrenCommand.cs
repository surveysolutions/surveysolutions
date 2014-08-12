﻿using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "CloneGroupWithoutChildren")]
    public class CloneGroupWithoutChildrenCommand : FullGroupDataCommand
    {
        public CloneGroupWithoutChildrenCommand(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, string variableName, Guid? rosterSizeQuestionId, string description, string condition,
            Guid? parentGroupId, Guid sourceGroupId, int targetIndex, bool isRoster, RosterSizeSourceType rosterSizeSource,
            string[] rosterFixedTitles, Guid? rosterTitleQuestionId)
            : base(
                questionnaireId, groupId, responsibleId, title, variableName, rosterSizeQuestionId, description, condition, isRoster, rosterSizeSource,
                rosterFixedTitles, rosterTitleQuestionId)
        {
            this.ParentGroupId = parentGroupId;
            this.SourceGroupId = sourceGroupId;
            this.TargetIndex = targetIndex;
        }

        public Guid? ParentGroupId { get; private set; }
        public Guid SourceGroupId { get; private set; }
        public int TargetIndex { get; private set; }
    }
}