using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class DateTimeQuestion : AbstractQuestion
    {
        public override QuestionType QuestionType => QuestionType.DateTime;
    }
}
