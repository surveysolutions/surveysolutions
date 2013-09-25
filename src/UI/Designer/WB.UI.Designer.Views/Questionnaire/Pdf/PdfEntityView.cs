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

        public int Depth { get; set; }

        public PdfEntityView Parent { get; set; }

        public List<PdfEntityView> Children { get; set; }

        public string ItemNumber
        {
            get
            {
                return string.Join(".", this.QuestionNumberSections);
            }
        }

        private List<int> questionNumberSections = null;

        protected List<int> QuestionNumberSections 
        {
            get
            {
                if (questionNumberSections == null)
                {
                    questionNumberSections = new List<int>();
                    var parent = this.Parent;
                    var currentItem = this;
                    while (parent != null)
                    {
                        var currentItemNumber = parent.Children.IndexOf(currentItem) + 1;
                        questionNumberSections.Insert(0, currentItemNumber);
                        currentItem = parent;
                        parent = parent.Parent;
                    }
                }

                return questionNumberSections;
            }
        }
    }
}