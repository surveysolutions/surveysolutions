using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class InterviewStatistics
    {
        public InterviewStatistics(IEnumerable<QuestionViewModel> questions)
        {
            Update(questions);
        }

        public IList<QuestionViewModel> AnsweredQuestions
        {
            get
            {
                return answeredQuestions;
            }
        }

        private IList<QuestionViewModel> answeredQuestions;

        public IList<QuestionViewModel> UnansweredQuestions
        {
            get
            {
                return unansweredQuestions;
            }
        }

        private IList<QuestionViewModel> unansweredQuestions;

        public IList<QuestionViewModel> InvalidQuestions
        {
            get
            {
                return invalidQuestions;
            }
        }

        private IList<QuestionViewModel> invalidQuestions;

        public void Update(IEnumerable<QuestionViewModel> questions)
        {
            answeredQuestions = questions.Where(q => q.IsEnabled() && q.Status.HasFlag(QuestionStatus.Answered)).ToList();
            unansweredQuestions = questions.Where(q => q.IsEnabled() && !q.Status.HasFlag(QuestionStatus.Answered)).ToList();
            invalidQuestions = questions.Where(q => q.IsEnabled() && !q.Status.HasFlag(QuestionStatus.Valid)).ToList();
        }
    }
}
