using System;

namespace RavenQuestionnaire.Web.Models
{
    public class MoveItemModel
    {
        public string questionnaireId { get; set; }

        public Guid publicKey { get; set; }

        public Guid? groupGuid { get; set; }

        public Guid? afterGuid { get; set; }
    }
}