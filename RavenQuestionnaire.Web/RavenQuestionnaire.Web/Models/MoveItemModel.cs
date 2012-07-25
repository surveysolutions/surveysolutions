using System;

namespace RavenQuestionnaire.Web.Models
{
    public class MoveItemModel
    {
        public Guid questionnaireId { get; set; }

        public Guid publicKey { get; set; }

        public Guid? groupGuid { get; set; }

        public Guid? afterGuid { get; set; }
    }
}