using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class JsonBundleCommandResponse
    {
        public List<JsonCommandResponse> CommandStatuses { get; set; }
        public JsonBundleCommandResponse()
        {
            this.CommandStatuses = new List<JsonCommandResponse>();
        }
    }
}