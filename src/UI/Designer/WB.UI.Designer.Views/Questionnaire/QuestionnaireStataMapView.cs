using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Designer.Views.Questionnaire
{
    public class QuestionnaireStataMapView
    {
        public QuestionnaireStataMapView()
        {
            StataMap = new List<KeyValuePair<Guid, string>>();
        }

        public QuestionnaireStataMapView(QuestionnaireDocument doc)
        {
            this.StataMap = doc.GetAllQuestions<AbstractQuestion>()
                               .Select(q => new KeyValuePair<Guid, string>(q.PublicKey, q.StataExportCaption))
                               .ToList();
        }

        public List<KeyValuePair<Guid, string>> StataMap { get; set; }
    }
}
