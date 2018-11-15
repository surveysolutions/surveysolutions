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

        private QuestionApiItem ToQuestionApiView(InterviewTreeQuestion question) =>
            new QuestionApiItem(question.VariableName, question.Identity, question.GetAnswerAsString(CultureInfo.InvariantCulture));
        
        [DataMember(IsRequired = true)]
        [Required]
        public List<QuestionApiItem> Answers { set; get; }
    }
}
