using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.UI.Headquarters.Models.Api.Interview;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class InterviewApiDetails
    {
        public InterviewApiDetails(IStatefulInterview interview)
        {
            this.Answers = interview.GetAllInterviewNodes().OfType<InterviewTreeQuestion>()
                                                           .Select(ToQuestionApiView)
                                                           .ToList();
        }

        private static string GetApiAnswerString(InterviewTreeQuestion question)
        {
            if (question.IsTextList)
            {
                var answer = (question.InterviewQuestion as InterviewTreeTextListQuestion)?.GetAnswer();
                return answer == null ? null : string.Join("|", answer.Rows.Select(r => r.Text));
            }

            if (question.IsMultiLinkedToList)
            {
                var linkedToListQuestion = question.InterviewQuestion as InterviewTreeMultiOptionLinkedToListQuestion;
                var multiToListAnswers = linkedToListQuestion?.GetAnswer()?.ToDecimals()?.ToHashSet();
                var refListQuestion = question.Tree.GetTreeNodeByLevelOrNull(
                    linkedToListQuestion?.LinkedSourceId ?? Guid.Empty,
                    question.Identity) as InterviewTreeQuestion;
                var refListQuestionAllOptions = (refListQuestion?.InterviewQuestion as InterviewTreeTextListQuestion)
                    ?.GetAnswer()?.Rows;
                var refListOptions = refListQuestionAllOptions
                    ?.Where(x => multiToListAnswers?.Contains(x.Value) ?? false)
                    .ToArray();
                return refListOptions == null ? string.Empty : string.Join("|", refListOptions.Select(o => o.Text));
            }

            return question.GetAnswerAsString(CultureInfo.InvariantCulture);
        }

        private QuestionApiItem ToQuestionApiView(InterviewTreeQuestion question) =>
            new QuestionApiItem(question.VariableName, question.Identity, GetApiAnswerString(question));
        
        [DataMember(IsRequired = true)]
        [Required]
        public List<QuestionApiItem> Answers { set; get; }
    }
}
