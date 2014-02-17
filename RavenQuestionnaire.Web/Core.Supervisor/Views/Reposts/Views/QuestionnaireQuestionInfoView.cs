using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class QuestionnaireQuestionInfoView
    {
        public IEnumerable<QuestionnaireQuestionInfoItem> Variables { get; set; }
    }

    public class QuestionnaireQuestionInfoItem
    {
        public string Variable { get; set; }
        public QuestionType Type { get; set; }
        public Guid Id { get; set; }
    }
}
