using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireView
    {
        private IEnumerable<ICompositeView> children;

        public QuestionnaireView(QuestionnaireDocument doc, IEnumerable<SharedPersonView> sharedPersons)
        {
            this.Source = doc;
            this.SharedPersons = sharedPersons.ToReadOnlyCollection();
        }

        public QuestionnaireDocument Source { get; }
        public IReadOnlyCollection<SharedPersonView> SharedPersons { get; }

        public IEnumerable<ICompositeView> Children
        {
            get
            {
                return this.children
                       ?? (this.children =
                           this.Source.Children.Cast<IGroup>().Select(@group => new EditGroupView(@group, null, 0)).ToList());
            }
        }

        public Guid? CreatedBy => this.Source.CreatedBy;

        public Guid PublicKey => this.Source.PublicKey;

        public string Title => this.Source.Title;

        public bool IsPublic => this.Source.IsPublic;
    }
}

