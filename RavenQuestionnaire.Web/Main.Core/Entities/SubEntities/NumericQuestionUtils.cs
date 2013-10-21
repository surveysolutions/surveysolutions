namespace Main.Core.Entities.SubEntities
{
    public static class NumericQuestionUtils
    {
        public static QuestionType GetQuestionTypeFromIsAutopropagatingParameter(bool isAutopropagating)
        {
            return isAutopropagating ? QuestionType.AutoPropagate : QuestionType.Numeric;
        }
    }
}
