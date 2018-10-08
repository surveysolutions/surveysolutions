using System.Diagnostics;

namespace WB.Services.Export.Interview.Entities
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewTextListAnswer
    {
        public InterviewTextListAnswer(decimal value, string answer)
        {
            this.Value = value;
            this.Answer = answer;
        }

        public decimal Value { get; set; }

        public string Answer { get; set; }

        public override bool Equals(object obj)
        {
            var target = obj as InterviewTextListAnswer;
            if (target == null) return false;

            return this.Value == target.Value && this.Answer == target.Answer;
        }

        public override int GetHashCode() => this.Answer.GetHashCode() ^ this.Value.GetHashCode();
        public override string ToString() => $"{this.Value} => {this.Answer}";
    }
}
