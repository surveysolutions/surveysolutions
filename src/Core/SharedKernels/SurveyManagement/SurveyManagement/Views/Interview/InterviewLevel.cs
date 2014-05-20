using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewLevel
    {
        public InterviewLevel()
        {
            this.ScopeVectors = new Dictionary<ValueVector<Guid>, int?>();
            this.Questions = new List<InterviewQuestion>();
            this.DisabledGroups = new HashSet<Guid>();
            this.RosterRowTitles = new Dictionary<Guid, string>();
            this.RosterTitleQuestionIdToRosterIdMap = new Dictionary<Guid, List<Guid>>();
            this.RosterTitleQuestionDescriptions = new Dictionary<Guid, RosterTitleQuestionDescription>();
        }
        public InterviewLevel(ValueVector<Guid> scopeVector, int? sortIndex, decimal[] vector)
            : this()
        {
            this.ScopeVectors = new Dictionary<ValueVector<Guid>, int?>() { { scopeVector, sortIndex } };
            this.RosterVector = vector;
        }

        public decimal[] RosterVector { get; set; }
        public Dictionary<ValueVector<Guid>, int?> ScopeVectors { get; private set; }
        private List<InterviewQuestion> Questions { get; set; }
        public HashSet<Guid> DisabledGroups { get; private set; }
        public Dictionary<Guid, string> RosterRowTitles { set; get; }
        public Dictionary<Guid, List<Guid>> RosterTitleQuestionIdToRosterIdMap { get; private set; }
        public Dictionary<Guid, RosterTitleQuestionDescription> RosterTitleQuestionDescriptions { get; private set; }

        public IEnumerable<InterviewQuestion> GetAllQuestions()
        {
            return this.Questions;
        }

        public InterviewQuestion GetQuestion(Guid questionId)
        {
            #warning TLK: I put all existing queries to this method to highlight that queries are not optimal
            return this.Questions.FirstOrDefault(question => question.Id == questionId);
        }

        public InterviewQuestion GetOrCreateQuestion(Guid questionId)
        {
            var answeredQuestion = this.GetQuestion(questionId);
            if (answeredQuestion == null)
            {
                answeredQuestion = new InterviewQuestion(questionId);
                this.Questions.Add(answeredQuestion);
            }
            return answeredQuestion;
        }
    }
}
