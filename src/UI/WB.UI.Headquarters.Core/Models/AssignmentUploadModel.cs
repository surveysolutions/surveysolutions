using System;
using Microsoft.AspNetCore.Http;

namespace WB.UI.Headquarters.Models
{
    public class AssignmentUploadModel
    {
        public string QuestionnaireId { get; set; }
        public Guid? ResponsibleId { get; set; }
        public IFormFile File { get; set; }
        public AssignmentUploadType Type { get; set; }
    }
}
