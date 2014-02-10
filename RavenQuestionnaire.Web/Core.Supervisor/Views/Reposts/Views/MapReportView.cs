using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.EventHandler;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class MapReportView
    {
        public string[] Answers { get; set; }

        public MapReportView(AnswersByVariableCollection answersCollection)
        {
            this.Answers = answersCollection.Answers.Select(x => string.Join(";", x.Value.Values)).ToArray();
        }
    }
}
