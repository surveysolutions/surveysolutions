using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class RosterChanged : GroupEvent
    {
        public RosterSizeSourceType RosterSizeSource { get; private set; }
        public Guid? RosterSizeQuestionId { get; private set; }
        public string[] RosterFixedTitles { get; private set; }

        public RosterChanged(Guid responsibleId, Guid groupId, Guid? rosterSizeQuestionId, RosterSizeSourceType rosterSizeSource, string[] rosterFixedTitles)
            : base(responsibleId, groupId)
        {
            this.RosterSizeQuestionId = rosterSizeQuestionId;
            this.RosterSizeSource = rosterSizeSource;
            this.RosterFixedTitles = rosterFixedTitles;
        }
    }
}