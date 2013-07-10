using System;
using System.Collections.Generic;

namespace WB.UI.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionnaireView
    {
        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string Title { get; set; }

        public int ChaptersCount { get; set; }

        public int QuestionsCount { get; set; }

        public int QuestionsWithConditionsCount { get; set; }

        public int GroupsCount { get; set; }

        public List<PdfGroupView> Groups { get; set; }
    }
}