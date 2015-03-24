using System;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class FullGroupDataCommand : GroupCommand
    {
        protected FullGroupDataCommand(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, string variableName, Guid? rosterSizeQuestionId, string condition, bool isRoster,
            RosterSizeSourceType rosterSizeSource, Tuple<decimal, string>[] fixedRosterTitles, Guid? rosterTitleQuestionId)
            : base(questionnaireId, groupId, responsibleId)
        {
            this.VariableName = CommandUtils.SanitizeHtml(variableName, removeAllTags: true);
            this.Title = CommandUtils.SanitizeHtml(title, removeAllTags: true);
            this.IsRoster = isRoster;
            this.RosterSizeQuestionId = rosterSizeQuestionId;
            this.RosterSizeSource = rosterSizeSource;
            this.Condition = condition;
            this.FixedRosterTitles = fixedRosterTitles == null
                ? null
                : fixedRosterTitles
                    .Select(x => new Tuple<decimal,string>(x.Item1,CommandUtils.SanitizeHtml(x.Item2, removeAllTags: true)))
                    .ToArray();
            this.RosterTitleQuestionId = rosterTitleQuestionId;
        }

        public string Title { get; private set; }
        public string VariableName { get; private set; }
        public bool IsRoster { get; private set; }
        public Guid? RosterSizeQuestionId { get; private set; }
        public RosterSizeSourceType RosterSizeSource { get; private set; }
        public Tuple<decimal, string>[] FixedRosterTitles { get; private set; }
        public Guid? RosterTitleQuestionId { get; private set; }

        public string Description { get; private set; }

        public string Condition { get; set; }
    }
}