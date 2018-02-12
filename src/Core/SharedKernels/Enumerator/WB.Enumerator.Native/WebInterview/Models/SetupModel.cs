using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Enumerator.Native.WebInterview.Models
{
    public class SetupModel
    {
        public string QuestionnaireTitle { get; set; }
        public bool UseCaptcha { get; set; }
        public int AssignmentsCount { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public string QuestionnaireFullName { get; set; }
        public string SurveySetupUrl { get; set; }
        public List<DropdownItem> TextOptions { get; set; }
    }
}