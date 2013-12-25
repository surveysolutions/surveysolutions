using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class FullGroupDataCommand : GroupCommand
    {
        protected FullGroupDataCommand(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, Guid? rosterSizeQuestionId, string description, string condition, bool isRoster,
            RosterSizeSourceType rosterSizeSource, string[] rosterFixedTitles, Guid? rosterTitleQuestionId)
            : base(questionnaireId, groupId, responsibleId)
        {
            this.Title = title;

            this.IsRoster = isRoster;
            this.RosterSizeQuestionId = rosterSizeQuestionId;
            this.RosterSizeSource = rosterSizeSource;
            this.Description = description;
            this.Condition = condition;
            this.RosterFixedTitles = rosterFixedTitles;
            this.RosterTitleQuestionId = rosterTitleQuestionId;
        }

        public string Title { get; private set; }

        public bool IsRoster { get; private set; }
        public Guid? RosterSizeQuestionId { get; private set; }
        public RosterSizeSourceType RosterSizeSource { get; private set; }
        public string[] RosterFixedTitles { get; private set; }
        public Guid? RosterTitleQuestionId { get; private set; }

        public string Description { get; private set; }

        public string Condition { get; set; }
    }
}