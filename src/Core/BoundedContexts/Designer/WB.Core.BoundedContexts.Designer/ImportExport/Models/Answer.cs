using System;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    public class Answer
    {
        public string Text { get; set; } = String.Empty;

        public int Code { get; set; }

        public int? ParentCode { get; set; }
    }
}
