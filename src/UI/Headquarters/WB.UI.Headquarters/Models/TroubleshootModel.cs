using System;
using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Models
{
    public class TroubleshootModel
    {
        [RegularExpression("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", 
            ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = "TroubleshootModel_InterviewId_ErrorMessage")]
        public Guid InterviewId { get; set; }
    }
}