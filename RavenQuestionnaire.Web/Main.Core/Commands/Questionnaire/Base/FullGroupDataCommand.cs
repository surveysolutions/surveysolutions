using System;
using Main.Core.Entities.SubEntities;

namespace Main.Core.Commands.Questionnaire.Base
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

        public string Title { get; set; }

        public Propagate PropagationKind { get; set; }

        public string Description { get; set; }

        public string Condition { get; set; }
    }
}