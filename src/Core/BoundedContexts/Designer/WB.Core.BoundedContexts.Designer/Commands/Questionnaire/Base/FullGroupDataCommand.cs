using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class FullGroupDataCommand : GroupCommand
    {
        protected FullGroupDataCommand(Guid questionnaireId, Guid groupId, string title, Propagate propagationKind, string description, string condition)
            : base(questionnaireId, groupId)
        {
            this.Title = title;
            this.PropagationKind = propagationKind;
            this.Description = description;
            this.Condition = condition;
        }

        public string Title { get; private set; }

        public Propagate PropagationKind { get; private set; }

        public string Description { get; private set; }

        public string Condition { get; set; }
    }
}