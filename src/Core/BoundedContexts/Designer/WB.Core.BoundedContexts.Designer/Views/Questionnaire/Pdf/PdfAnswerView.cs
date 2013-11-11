﻿using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfAnswerView
    {
        public string AnswerValue { get; set; }

        public string Title { get; set; }

        public AnswerType AnswerType { get; set; }
    }
}