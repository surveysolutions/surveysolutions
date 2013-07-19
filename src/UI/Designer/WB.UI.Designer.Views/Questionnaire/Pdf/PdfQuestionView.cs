using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionView : PdfEntityView
    {
        public PdfQuestionView()
        {
            this.Answers = new List<PdfAnswerView>();
        }

        public QuestionType QuestionType { get; set; }

        public List<PdfAnswerView> Answers { get; set; }

        public bool HasCodition { get; set; }
    }
}