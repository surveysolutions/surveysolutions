using System;

namespace WB.UI.Headquarters.Models
{
    public class JsonCommandResponse: JsonBaseResponse
    {
        public Guid CommandId { get; set; }
        public string DomainException { get; set; }
    }
}