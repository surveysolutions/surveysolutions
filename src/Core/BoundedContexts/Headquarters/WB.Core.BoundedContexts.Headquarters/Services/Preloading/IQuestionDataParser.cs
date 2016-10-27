using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    internal interface IQuestionDataParser
    {
        ValueParsingResult TryParse(string answer, string columnName, IQuestion question, out  object value);

        AbstractAnswer BuildAnswerFromStringArray(Tuple<string, string>[] answersWithColumnName, IQuestion question);
    }
}
