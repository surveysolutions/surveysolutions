using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class FullGroupDataCommand : GroupCommand
    {
        protected FullGroupDataCommand(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, Propagate propagationKind, Guid? rosterSizeQuestionId, string description, string condition)
            : base(questionnaireId, groupId, responsibleId)
        {
            this.Title = title;
            this.PropagationKind = propagationKind;
            this.RosterSizeQuestionId = rosterSizeQuestionId;
            this.Description = description;
            this.Condition = condition;
        }

        public string Title { get; private set; }

        public Propagate PropagationKind { get; private set; }

        public Guid? RosterSizeQuestionId { get; private set; }

        public string Description { get; private set; }

        public string Condition { get; set; }
    }
}