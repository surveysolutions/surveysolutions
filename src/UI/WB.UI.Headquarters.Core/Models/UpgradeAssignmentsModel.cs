using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Models.ComponentModels;

namespace WB.UI.Headquarters.Models
{
    public class UpgradeAssignmentsModel
    {
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public string SurveySetupUrl { get; set; }
        public List<ComboboxOptionModel> Questionnaires { get; set; }
    }
}
