using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IIdentifyingAnswerConverter
    {
        AbstractAnswer GetAbstractAnswer(QuestionType questionType, string answer);
    }
}