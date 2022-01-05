using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace Main.Core.Entities.SubEntities.Question
{
    public class NumericQuestion : AbstractQuestion, INumericQuestion
    {
        public override QuestionType QuestionType => QuestionType.Numeric;
        
        public bool IsInteger { get; set; }
        
        public int? CountOfDecimalPlaces { get; set; }

        public bool UseFormatting { get; set; }
    }
}
