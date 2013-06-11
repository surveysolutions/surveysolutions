using System;

namespace WB.UI.Designer.Pdf
{
    public class TitlePageDto
    {
        public string SurveyName { get; set; }

        public string CreationDate { get; set; }

        public int ChaptersCount { get; set; }

        public int QuestionsCount { get; set; }

        public int QuestionsWithConditionsCount { get; set; }
    }
}