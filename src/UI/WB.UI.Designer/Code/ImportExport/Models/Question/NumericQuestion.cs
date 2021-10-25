using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.UI.Designer.Code.ImportExport.Models.Question
{
    public class NumericQuestion : AbstractQuestion
    {
        public bool IsInteger { get; set; }
        
        public int? CountOfDecimalPlaces { get; set; }

        public bool UseFormatting { get; set; }
    
        public List<Answer>? Answers { get; set; }
    }
}
