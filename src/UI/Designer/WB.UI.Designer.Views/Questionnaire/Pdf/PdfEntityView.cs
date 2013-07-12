using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WB.UI.Designer.Views.Questionnaire.Pdf
{
    [DebuggerDisplay("Id = {Id}, Title = {Title}")]
    public abstract class PdfEntityView
    {
        protected PdfEntityView()
        {
            this.Children = new List<PdfEntityView>();
        }

        public string Title { get; set; }

        public Guid Id { get; set; }

        public PdfEntityView Parent { get; set; } 

        public List<PdfEntityView> Children { get; set; }
    }
}