using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.Models.Api.Interview
{
    public class QuestionApiItem
    {
        public QuestionApiItem(string variableName, Identity questionId, string answer)
        {
            VariableName = variableName;
            QuestionId = questionId;
            Answer = answer;
        }

        [DataMember(IsRequired = true)]
        [Required]
        public string VariableName { set; get; }
        
        [DataMember(IsRequired = true)]
        [Required]
        public Identity QuestionId { get; set; }

        [DataMember(IsRequired = false)]
        public string Answer { set; get; }
    }
}
