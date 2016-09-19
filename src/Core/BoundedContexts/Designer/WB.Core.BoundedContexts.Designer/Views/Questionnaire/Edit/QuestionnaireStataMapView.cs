using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireStataMapView
    {
        public QuestionnaireStataMapView()
        {
            StataMap = new List<KeyValuePair<Guid, string>>();
        }

        public QuestionnaireStataMapView(QuestionnaireDocument doc)
        {
            this.StataMap = doc.GetEntitiesByType<AbstractQuestion>()
                               .Select(q => new KeyValuePair<Guid, string>(q.PublicKey, q.StataExportCaption))
                               .ToList();
        }

        public List<KeyValuePair<Guid, string>> StataMap { get; set; }
    }
}
