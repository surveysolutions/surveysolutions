using System;
using System.ComponentModel.DataAnnotations;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class RevalidateModel
    {
        [RegularExpression(
            "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", 
            ErrorMessage = "Enter valid Guid in format xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx, please")]
        public Guid InterviewId { get; set; }
    }
}