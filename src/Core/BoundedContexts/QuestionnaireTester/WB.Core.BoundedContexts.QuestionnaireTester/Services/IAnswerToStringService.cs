using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface IAnswerToStringService
    {
        string AnswerToString(BaseQuestionModel question, BaseInterviewAnswer answer);
    }
}