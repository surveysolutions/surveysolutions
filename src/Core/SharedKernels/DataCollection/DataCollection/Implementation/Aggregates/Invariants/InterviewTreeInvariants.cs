using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewTreeInvariants
    {
        public InterviewTreeInvariants(InterviewTree interviewTree)
        {
            this.InterviewTree = interviewTree;
        }

        public InterviewTree InterviewTree { get; }

        public void ThrowIfRosterVectorIsIncorrect(Guid questionId, RosterVector rosterVector)
        {
            if (rosterVector == null)
                throw new InterviewException(
                    $"Roster information for question is missing. " +
                    $"Roster vector cannot be null. " +
                    $"Question ID: {questionId.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");

            var questions = this.InterviewTree.FindQuestions(questionId);
            var rosterVectors = questions.Select(question => question.Identity.RosterVector).ToList();

            if (!rosterVectors.Contains(rosterVector))
                throw new InterviewException(
                    $"Roster information for question is incorrect. " +
                    $"No questions found for roster vector {rosterVector}. " +
                    $"Available roster vectors: {string.Join(", ", rosterVectors)}. " +
                    $"Question ID: {questionId.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");
        }
    }
}