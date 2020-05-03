using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public static class DashboardItemsExtensions
    {
        public static IReadOnlyCollection<AssignmentDocument> SortAssignments(this IReadOnlyCollection<AssignmentDocument> assignments)
        {
            var documents = assignments
                .Select(a => new
                {
                    Questionnaire = a.Title,
                    Assignment = a,
                    Answers = GetAnswersString(a)
                })
                .OrderBy(a => a.Questionnaire)
                .ThenBy(a => a.Answers)
                .Select(a => a.Assignment);

            return documents.ToReadOnlyCollection();
        }

        private static string GetAnswersString(AssignmentDocument a)
        {
            var answers = a.IdentifyingAnswers.Select(answer =>
            {
                if (answer.SerializedAnswer.Contains("NumericIntegerAnswer", StringComparison.Ordinal) ||
                    answer.SerializedAnswer.Contains("NumericRealAnswer", StringComparison.Ordinal))
                {
                    return answer.AnswerAsString.PadLeft(20, '0');
                }

                return answer.AnswerAsString;
            });
            return string.Join('-', answers);
        }

    }
}
