using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class RosterChanged : GroupEvent
    {
        public RosterSizeSourceType RosterSizeSource { get; set; }
        public Guid? RosterSizeQuestionId { get; set; }

        [Obsolete]
        public string[] RosterFixedTitles
        {
            set
            {
                if (value != null && value.Any())
                {
                    FixedRosterTitles = value.Select((t, i) => new FixedRosterTitle(i, t)).ToArray();
                }
                else
                {
                    FixedRosterTitles = new FixedRosterTitle[0];
                }
            }
        }

        public FixedRosterTitle[] FixedRosterTitles { get; set; }

        public Guid? RosterTitleQuestionId { get; set; }

        public RosterChanged(Guid responsibleId, Guid groupId)
            : base(responsibleId, groupId)
        {
            FixedRosterTitles = new FixedRosterTitle[0];
        }
    }
}