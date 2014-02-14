using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Supervisor.Views.Reposts.InputModels
{
    public class QuestionnaireQuestionInfoInputModel
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
    }
}
