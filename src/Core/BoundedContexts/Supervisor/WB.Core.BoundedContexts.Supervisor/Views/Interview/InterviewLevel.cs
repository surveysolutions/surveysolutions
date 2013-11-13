using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Supervisor.Views.Interview
{
    public class InterviewLevel
    {
        public InterviewLevel(Guid scopeId, int[] vector)
        {
            this.ScopeIds = new HashSet<Guid>(new[] {scopeId});
            this.RosterVector = vector;
            this.Questions = new List<InterviewQuestion>();
            this.DisabledGroups = new HashSet<Guid>();
        }

        public int[] RosterVector { get; private set; }
        public HashSet<Guid> ScopeIds { get; private set; }
        private List<InterviewQuestion> Questions { get; set; }
        public HashSet<Guid> DisabledGroups { get; private set; }

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
