using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class AreaQuestion : AbstractQuestion
    {
        public override QuestionType QuestionType => QuestionType.Area;
    }
}
