using System;
using System.Collections.Generic;

namespace WB.UI.Headquarters.Models
{
    public class QuestionnaireExposedVariablesModel
    {
        public QuestionnaireExposedVariablesModel()
        {
        }

        public Guid QuestionnaireId { get; set; }
        public string Title { get; set; }
        public long Version { get; set; }

        public string DataUrl { get; set; }
        public string DesignerUrl { get; set; }
        public string QuestionnaireIdentity { get; set; }
        public bool IsObserving { get; set; }
    }
}
