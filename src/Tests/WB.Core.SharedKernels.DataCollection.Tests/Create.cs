using System;
using System.Collections.Generic;
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

        public static EnablementChanges EnablementChanges(List<Identity> groupsToBeDisabled = null, List<Identity> groupsToBeEnabled = null,
            List<Identity> questionsToBeDisabled = null, List<Identity> questionsToBeEnabled = null)
        {
            return new EnablementChanges(
                groupsToBeDisabled ?? new List<Identity>(),
                groupsToBeEnabled ?? new List<Identity>(),
                questionsToBeDisabled ?? new List<Identity>(),
                questionsToBeEnabled ?? new List<Identity>());
        }

        public static Identity Identity(Guid id, decimal[] rosterVector)
        {
            return new Identity(id, rosterVector);
        }
    }
}