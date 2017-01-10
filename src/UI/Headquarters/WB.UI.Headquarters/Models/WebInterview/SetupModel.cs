using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class SetupModel
    {
        public string QuestionnaireTitle { get; set; }
        public long QuestionnaireVersion { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public Guid? ResponsibleId { get; set; }
    }
}