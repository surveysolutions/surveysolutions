using System.Collections.Generic;

namespace WB.UI.Designer.Views.Questionnaire.Pdf
{
    public class PdfGroupView : PdfEntityView
    {
        public int Depth { get; set; }

        public IList<PdfEntityView> Children { get; set; }
    }
}