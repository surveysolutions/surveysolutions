using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class SetupModel
    {
        public string QuestionnaireTitle { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string WebInterviewLink { get; set; }
        public Guid? ResponsibleId { get; set; }
        public bool UseCaptcha { get; set; }
    }
}