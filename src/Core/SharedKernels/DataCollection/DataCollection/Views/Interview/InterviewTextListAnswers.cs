using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Views.Interview
{
    public class InterviewTextListAnswers
    {
        public InterviewTextListAnswers() { }

        public InterviewTextListAnswers(IEnumerable<Tuple<decimal, string>> answers)
        {
            this.Answers = answers.Select(a => new InterviewTextListAnswer(a.Item1, a.Item2)).ToArray();
        }

        public InterviewTextListAnswer[] Answers { get; set; }
    }
}