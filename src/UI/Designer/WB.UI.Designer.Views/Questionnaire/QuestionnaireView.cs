using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;

namespace WB.UI.Designer.Views.Questionnaire
{
    public class QuestionnaireView
    {
        public QuestionnaireView(IQuestionnaireDocument doc)
        {
            this.PublicKey = doc.PublicKey;
            this.Title = doc.Title;
            this.PublicKey = doc.PublicKey;
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;

            this.Children = ConvertChildrenFromQuestionnaireDocument(doc).ToList();
        }

        public List<ICompositeView> Children { get; set; }
        public Guid? Parent { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }
        public Guid PublicKey { get; set; }
        public string Title { get; set; }

        private static IEnumerable<ICompositeView> ConvertChildrenFromQuestionnaireDocument(IQuestionnaireDocument doc)
        {
            return doc.Children
                .Cast<IGroup>()
                .Select(@group => new GroupView(doc, @group));
        }
    }
}