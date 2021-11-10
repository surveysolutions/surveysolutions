using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models.Question
{
    public class NumericQuestion : AbstractQuestion
    {
        public bool IsInteger { get; set; }
        
        public int? DecimalPlaces { get; set; }

        public bool UseThousandsSeparator { get; set; }
    
        public List<Answer>? Answers { get; set; }
    }
}
