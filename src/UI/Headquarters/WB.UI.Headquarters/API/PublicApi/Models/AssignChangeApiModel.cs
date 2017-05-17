using System;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class AssignChangeApiModel
    {
        public Guid Id { set; get; }
        public Guid? ResponsibleId { set; get; }
        public string ResponsibleName { set; get; }
    }
}
