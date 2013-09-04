using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class ExportedQuestion
    {
        public ExportedQuestion(Guid questionId, string[] answers)
        {
            QuestionId = questionId;
            Answers = answers;
        }

        public Guid QuestionId { get; private set; }
        public string[] Answers { get; private set; }
    }
}
