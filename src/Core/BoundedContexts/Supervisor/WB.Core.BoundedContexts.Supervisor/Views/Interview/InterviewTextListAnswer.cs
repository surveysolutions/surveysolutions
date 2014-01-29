using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.Interview
{
    public class InterviewTextListAnswer
    {
        public InterviewTextListAnswer(decimal value, string answer)
        {
            this.Value = value;
            this.Answer = answer;
        }

        public decimal Value { get; private set; }
        public string Answer { get; private set; }
    }
}
