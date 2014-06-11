using System.Collections.Generic;

namespace WB.UI.Headquarters.Models
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