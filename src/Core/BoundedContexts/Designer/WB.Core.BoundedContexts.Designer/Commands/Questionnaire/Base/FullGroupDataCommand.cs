using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class FullGroupDataCommand : GroupCommand
    {
        protected FullGroupDataCommand(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, string variableName, Guid? rosterSizeQuestionId, string condition, bool hideIfDisabled, bool isRoster,
            RosterSizeSourceType rosterSizeSource, FixedRosterTitleItem[]? fixedRosterTitles, Guid? rosterTitleQuestionId, RosterDisplayMode displayMode)
            : base(questionnaireId, groupId, responsibleId)
        {
            this.VariableName = CommandUtils.SanitizeHtml(variableName, removeAllTags: true);
            this.Title = CommandUtils.SanitizeHtml(title, removeAllTags: true);
            this.IsRoster = isRoster;
            this.DisplayMode = displayMode;
            this.RosterSizeQuestionId = rosterSizeQuestionId;
            this.RosterSizeSource = rosterSizeSource;
            this.Condition = condition;
            this.HideIfDisabled = hideIfDisabled;
            this.FixedRosterTitles = fixedRosterTitles == null
                ? null
                : fixedRosterTitles
                    .Select(x => new FixedRosterTitleItem(x.Value, CommandUtils.SanitizeHtml(x.Title, removeAllTags: true)))
                    .ToArray();
            this.RosterTitleQuestionId = rosterTitleQuestionId;
        }

        public string Title { get; private set; }
        public string VariableName { get; private set; }
        public bool IsRoster { get; private set; }
        public RosterDisplayMode DisplayMode { get; private set; }
        public Guid? RosterSizeQuestionId { get; private set; }
        public RosterSizeSourceType RosterSizeSource { get; private set; }
        public FixedRosterTitleItem[]? FixedRosterTitles { get; private set; }
        public Guid? RosterTitleQuestionId { get; private set; }
        public string? Description { get; private set; }

        public string Condition { get; private set; }
        public bool HideIfDisabled { get; private set; }
    }
}
