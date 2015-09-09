using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class MultimediaQuestionUpdated : AbstractQuestionUpdated
    {
        public QuestionScope QuestionScope { get; set; }
    }
}
