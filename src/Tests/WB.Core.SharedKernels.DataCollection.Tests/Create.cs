using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Tests
{
    internal static class Create
    {
        public static Questionnaire Questionnaire(Guid creatorId, QuestionnaireDocument document)
        {
            return new Questionnaire(new Guid(), document, false);
        }

        public static InterviewState InterviewState(InterviewStatus? status =null,List<AnswerComment> answerComments = null)
        {
            return new InterviewState(Guid.NewGuid(), 1, status ?? InterviewStatus.SupervisorAssigned, new Dictionary<string, object>(),
                new Dictionary<string, Tuple<Guid, decimal[], decimal[]>>(), new Dictionary<string, Tuple<Guid, decimal[], decimal[][]>>(),
                new Dictionary<string, Tuple<decimal, string>[]>(), new HashSet<string>(),
                answerComments ?? new List<AnswerComment>(),
                new HashSet<string>(),
                new HashSet<string>(), new Dictionary<string, DistinctDecimalList>(), new HashSet<string>(
                    ), new HashSet<string>(), true);
        }
    }
}