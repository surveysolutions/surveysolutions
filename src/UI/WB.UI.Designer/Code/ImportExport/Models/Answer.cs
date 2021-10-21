using System;
using System.Collections.Generic;
using System.Globalization;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public class Answer
    {
        public string Text { get; set; } = String.Empty;

        public int Code { get; set; }

        public int? ParentCode { get; set; }
    }
}
