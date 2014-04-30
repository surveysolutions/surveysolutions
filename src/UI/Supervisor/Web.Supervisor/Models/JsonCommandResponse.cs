using System;
using Web.Supervisor.Controllers;

namespace Web.Supervisor.Models
{
    public class JsonCommandResponse: JsonBaseResponse
    {
        public Guid CommandId { get; set; }
        public string DomainException { get; set; }
    }
}