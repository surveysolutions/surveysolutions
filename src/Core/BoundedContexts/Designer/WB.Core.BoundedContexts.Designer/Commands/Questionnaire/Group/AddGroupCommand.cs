﻿using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "AddGroup")]
    public class AddGroupCommand : FullGroupDataCommand
    {
        public AddGroupCommand(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, string variableName, Guid? rosterSizeQuestionId, string condition,
            Guid? parentGroupId, bool isRoster, RosterSizeSourceType rosterSizeSource, string[] rosterFixedTitles, Guid? rosterTitleQuestionId)
            : base(
                questionnaireId, groupId, responsibleId, title, variableName, rosterSizeQuestionId, condition, isRoster, rosterSizeSource,
                rosterFixedTitles, rosterTitleQuestionId)
        {
            this.ParentGroupId = parentGroupId;
        }

        public Guid? ParentGroupId { get; private set; }
    }
}