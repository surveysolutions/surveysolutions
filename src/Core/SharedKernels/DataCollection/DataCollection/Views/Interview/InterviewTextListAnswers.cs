using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Views.Interview
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewTextListAnswers
    {
        public InterviewTextListAnswers() { }

        public InterviewTextListAnswers(IEnumerable<Tuple<decimal, string>> answers)
        {
            this.Answers = answers.Select(a => new InterviewTextListAnswer(a.Item1, a.Item2)).ToArray();
        }

        public InterviewTextListAnswer[] Answers { get; set; }

        public override int GetHashCode() => this.Answers?.Aggregate(0,
            (s, i) => s.GetHashCode() ^ (i?.GetHashCode() ?? 0)) ?? 0;

        public override bool Equals(object obj)
        {
            var target = obj as InterviewTextListAnswers;
            if (target == null) return false;

            return this.Answers?.SequenceEqual(target.Answers ?? new InterviewTextListAnswer[0]) ?? false;
        }

        public override string ToString()
            => string.Join(",", this.Answers.Select(x => x.ToString()));
    }
}