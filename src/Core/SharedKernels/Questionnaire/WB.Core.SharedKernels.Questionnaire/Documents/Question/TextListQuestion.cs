using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace Main.Core.Entities.SubEntities.Question
{
    public class TextListQuestion : AbstractQuestion, ITextListQuestion
    {
        public override QuestionType QuestionType => QuestionType.TextList;

        public int? MaxAnswerCount { get; set; }
        
        public static int MaxAnswerCountLimit => Constants.MaxLongRosterRowCount;
    }
}
