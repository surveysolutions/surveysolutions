using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
{
    public class AssignChangeApiModel
    {
        public Guid Id { set; get; }
        public Guid? ResponsibleId { set; get; }
        public string ResponsibleName { set; get; }
    }
}
