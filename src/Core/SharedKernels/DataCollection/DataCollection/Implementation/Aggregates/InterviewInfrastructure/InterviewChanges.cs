using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class InterviewChanges
    {
        public InterviewChanges(List<AnswerChange> interviewByAnswerChanges, 
            EnablementChanges enablementChanges, 
            ValidityChanges validityChanges,
            RosterCalculationData rosterCalculationData, 
            List<Identity> answersForLinkedQuestionsToRemove,
            List<RosterIdentity> rosterInstancesWithAffectedTitles, 
            string answerAsRosterTitle,
            List<Identity> changedQuestionTitles)
        {
            this.AnswerAsRosterTitle = answerAsRosterTitle;
            this.ChangedQuestionTitles = changedQuestionTitles;

            this.InterviewByAnswerChanges = interviewByAnswerChanges;

            this.EnablementChanges = enablementChanges;
            this.ValidityChanges = validityChanges;
            this.RosterCalculationData = rosterCalculationData;
            this.AnswersForLinkedQuestionsToRemove = answersForLinkedQuestionsToRemove;
            this.RosterInstancesWithAffectedTitles = rosterInstancesWithAffectedTitles;
        }

        public string AnswerAsRosterTitle { set; get; }
        public List<Identity> ChangedQuestionTitles { get; set; }

        public List<AnswerChange> InterviewByAnswerChanges { set; get; }
        public EnablementChanges EnablementChanges { set; get; }
        public ValidityChanges ValidityChanges { set; get; }
        public RosterCalculationData RosterCalculationData { set; get; }
        public List<Identity> AnswersForLinkedQuestionsToRemove { set; get; }
        public List<RosterIdentity> RosterInstancesWithAffectedTitles { set; get; }
    }
}
