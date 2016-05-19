using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class InterviewChanges
    {
        public InterviewChanges(
            List<AnswerChange> interviewByAnswerChanges, 
            EnablementChanges enablementChanges, 
            ValidityChanges validityChanges,
            RosterCalculationData rosterCalculationData, 
            List<Identity> answersForLinkedQuestionsToRemove,
            List<RosterIdentity> rosterInstancesWithAffectedTitles, 
            string answerAsRosterTitle,
            IEnumerable<Identity> changedQuestionTitles,
            IEnumerable<Identity> changedStaticTextTitles,
            ChangedLinkedOptions[] linkedQuestionOptionsChanges,
            VariableValueChanges variableValueChanges)
        {
            this.AnswerAsRosterTitle = answerAsRosterTitle;
            this.ChangedQuestionTitles = changedQuestionTitles?.ToArray();
            this.ChangedStaticTextTitles = changedStaticTextTitles?.ToArray();

            this.InterviewByAnswerChanges = interviewByAnswerChanges;

            this.EnablementChanges = enablementChanges;
            this.ValidityChanges = validityChanges;
            this.RosterCalculationData = rosterCalculationData;
            this.AnswersForLinkedQuestionsToRemove = answersForLinkedQuestionsToRemove;
            this.RosterInstancesWithAffectedTitles = rosterInstancesWithAffectedTitles;
            this.LinkedQuestionOptionsChanges = linkedQuestionOptionsChanges;
            this.VariableValueChanges = variableValueChanges;
        }

        public string AnswerAsRosterTitle { set; get; }
        public Identity[] ChangedQuestionTitles { get; set; }
        public Identity[] ChangedStaticTextTitles { get; set; }

        public List<AnswerChange> InterviewByAnswerChanges { set; get; }
        public EnablementChanges EnablementChanges { set; get; }
        public ValidityChanges ValidityChanges { set; get; }
        public ChangedLinkedOptions[] LinkedQuestionOptionsChanges { set; get; }
        public RosterCalculationData RosterCalculationData { set; get; }
        public List<Identity> AnswersForLinkedQuestionsToRemove { set; get; }
        public List<RosterIdentity> RosterInstancesWithAffectedTitles { set; get; }
        public VariableValueChanges VariableValueChanges { get; set; }
    }
}
