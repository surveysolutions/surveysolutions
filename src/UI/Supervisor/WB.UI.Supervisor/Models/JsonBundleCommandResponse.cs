using System.Collections.Generic;

namespace Web.Supervisor.Models
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