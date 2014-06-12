using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class JsonCommandResponse: JsonBaseResponse
    {
        public Guid CommandId { get; set; }
        public string DomainException { get; set; }
    }
}