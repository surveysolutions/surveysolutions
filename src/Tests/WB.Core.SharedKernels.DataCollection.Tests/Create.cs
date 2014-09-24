using System;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Tests
{
    internal static class Create
    {
        public static Questionnaire Questionnaire(Guid creatorId, QuestionnaireDocument document)
        {
            return new Questionnaire(new Guid(), document, false, string.Empty);
        }
    }
}