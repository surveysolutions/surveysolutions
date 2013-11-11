using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

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
            this.StataMap = doc.GetAllQuestions<AbstractQuestion>()
                               .Select(q => new KeyValuePair<Guid, string>(q.PublicKey, q.StataExportCaption))
                               .ToList();
        }

        public QuestionnaireStataMapView(EditQuestionnaireView doc)
        {
            this.StataMap = doc.Questions
                               .Select(q => new KeyValuePair<Guid, string>(q.Id, q.StataExportCaption))
                               .ToList();
        }

        public List<KeyValuePair<Guid, string>> StataMap { get; set; }
    }
}
