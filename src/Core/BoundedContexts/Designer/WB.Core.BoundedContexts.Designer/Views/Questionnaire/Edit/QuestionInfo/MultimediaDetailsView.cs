using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class MultimediaDetailsView: QuestionDetailsView
    {
        public MultimediaDetailsView()
        {
            Type = QuestionType.Multimedia;
        }
        public override sealed QuestionType Type { get; set; }
    }
}
