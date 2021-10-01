using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class GpsCoordinateQuestion : ExternalServiceQuestion
    {
        public override QuestionType QuestionType => QuestionType.GpsCoordinates;
    }
}
