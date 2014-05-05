using System;
using WB.UI.Supervisor.Controllers;

namespace WB.UI.Supervisor.Models
{
    public class JsonCommandResponse: JsonBaseResponse
    {
        public Guid CommandId { get; set; }
        public string DomainException { get; set; }
    }
}