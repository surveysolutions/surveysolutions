using System;

namespace WB.UI.Designer.Views.Questionnaire.Pdf
{
    public abstract class PdfEntityView
    {
        public string Title { get; set; }

        public Guid Id { get; set; }
    }
}