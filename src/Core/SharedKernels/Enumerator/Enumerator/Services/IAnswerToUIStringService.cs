using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IAnswerToUIStringService
    {
        string AnswerToUIString(BaseQuestionModel question, BaseInterviewAnswer answer);
    }
}