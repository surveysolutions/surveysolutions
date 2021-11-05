using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class TextQuestion : AbstractQuestion
    {
        public override QuestionType QuestionType => QuestionType.Text;

        public string? Mask { get; set; }
    }
}
