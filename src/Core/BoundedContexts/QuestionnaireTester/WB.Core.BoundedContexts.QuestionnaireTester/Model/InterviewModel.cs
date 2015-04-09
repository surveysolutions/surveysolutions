using System;
namespace WB.Core.BoundedContexts.QuestionnaireTester.Model
{
    public class InterviewModel
    {
        public Guid QuestionaryId
        {
            get { return Guid.NewGuid(); }
        }


        // TODO delete after implimentation, this is only dummy method
        public T GetAnswerOnQuestion<T>(Guid questionId)
        {
            return default(T);
        }
    }
}