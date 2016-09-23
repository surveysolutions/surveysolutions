using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class RosterChanged : Group
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
                    this.FixedRosterTitles = value.Select((t, i) => new FixedRosterTitle(i, t)).ToArray();
                }
                else
                {
                    this.FixedRosterTitles = new FixedRosterTitle[0];
                }
            }
        }

        public FixedRosterTitle[] FixedRosterTitles { get; set; }

        public Guid? RosterTitleQuestionId { get; set; }

        public RosterChanged(Guid responsibleId, Guid groupId)
            : base(responsibleId, groupId)
        {
            this.FixedRosterTitles = new FixedRosterTitle[0];
        }
    }
}