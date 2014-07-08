using System.Collections.Generic;
using WB.Core.Infrastructure.BaseStructures;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class InterviewChanges
    {
        public InterviewChanges(List<AnswerChange> interviewByAnswerChanges, EnablementChanges enablementChanges, ValidityChanges validityChanges,
            RosterCalculationData rosterCalculationData, List<Identity> answersForLinkedQuestionsToRemoveByDisabling,
            List<RosterIdentity> rosterInstancesWithAffectedTitles, string answerAsRosterTitle)
        {
            this.AnswerAsRosterTitle = answerAsRosterTitle;

            this.InterviewByAnswerChanges = interviewByAnswerChanges;

            this.EnablementChanges = enablementChanges;
            this.ValidityChanges = validityChanges;
            this.RosterCalculationData = rosterCalculationData;
            this.AnswersForLinkedQuestionsToRemoveByDisabling = answersForLinkedQuestionsToRemoveByDisabling;
            this.RosterInstancesWithAffectedTitles = rosterInstancesWithAffectedTitles;
        }

        public string AnswerAsRosterTitle { set; get; }

        public List<AnswerChange> InterviewByAnswerChanges { set; get; }
        public EnablementChanges EnablementChanges { set; get; }
        public ValidityChanges ValidityChanges { set; get; }
        public RosterCalculationData RosterCalculationData { set; get; }
        public List<Identity> AnswersForLinkedQuestionsToRemoveByDisabling { set; get; }
        public List<RosterIdentity> RosterInstancesWithAffectedTitles { set; get; }
    }
}
