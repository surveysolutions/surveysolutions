using System;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Designer.Models
{
    public class SharingNotificationModel
    {
        public Guid QiestionnaireId { get; set; }
        public string QiestionnaireTitle { get; set; }
        public ShareType ShareType { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string ActionPersonEmail { get; set; }
        public string SharedWithPersonEmail { get; set; }
        
    }
}