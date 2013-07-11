using System;
using System.Collections.Generic;

namespace WB.UI.Designer.Views.Questionnaire.Pdf
{
    public abstract class PdfEntityView
    {
        protected PdfEntityView()
        {
            this.Children = new List<PdfEntityView>();
        }

        public string Title { get; set; }

        public Guid Id { get; set; }

        public int Depth { get; set; }

        public List<PdfEntityView> Children { get; set; }
    }
}