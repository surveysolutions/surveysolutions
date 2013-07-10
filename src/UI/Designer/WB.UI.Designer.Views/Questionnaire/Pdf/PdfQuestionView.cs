using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionView : PdfEntityView
    {
        public QuestionType QuestionType { get; set; }

        public List<PdfAnswerView> Answers { get; set; }
    }
}