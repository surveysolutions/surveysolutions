using System;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class RosterChanged : GroupEvent
    {
        public RosterSizeSourceType RosterSizeSource { get; set; }
        public Guid? RosterSizeQuestionId { get; set; }

        [Obsolete]
        public string[] RosterFixedTitles { get; set; }

        private Tuple<decimal, string>[] fixedRosterTitles;
        public Tuple<decimal, string>[] FixedRosterTitles
        {
            get
            {
                if (fixedRosterTitles == null && RosterFixedTitles != null)
                {
                    fixedRosterTitles =
                        RosterFixedTitles.Select((title, index) => new Tuple<decimal, string>(index, title)).ToArray();
                }
                return fixedRosterTitles;
            }
            set { fixedRosterTitles = value; }
        }

        public Guid? RosterTitleQuestionId { get; set; }

        public RosterChanged(Guid responsibleId, Guid groupId)
            : base(responsibleId, groupId)
        {
        }
    }
}