namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Interview
{
    public static class QuestionStateExtensions
    {
        public static QuestionState Without(this QuestionState states, QuestionState stateToRemove)
        {
            return states & ~stateToRemove;
        }

        public static QuestionState With(this QuestionState states, QuestionState stateToAdd)
        {
            return states | stateToAdd;
        }
    }
}