using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

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
                FixedRosterTitles = new Dictionary<decimal, string>();

                if (value != null && value.Any())
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        FixedRosterTitles[i] = value[i];
                    }
                }
            }

        }

        public Dictionary<decimal, string> FixedRosterTitles{get; set;}

        public Guid? RosterTitleQuestionId { get; set; }

        public RosterChanged(Guid responsibleId, Guid groupId)
            : base(responsibleId, groupId)
        {
            FixedRosterTitles = new Dictionary<decimal, string>();
        }
    }
}