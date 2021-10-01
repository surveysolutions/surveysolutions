using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class AreaQuestion : ExternalServiceQuestion
    {
        public override QuestionType QuestionType => QuestionType.Area;
    }
}
