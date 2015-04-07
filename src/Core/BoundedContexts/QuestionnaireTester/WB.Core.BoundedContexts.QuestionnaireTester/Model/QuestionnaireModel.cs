using System;
using System.Collections.Generic;

using Main.Core.Entities.Composite;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Model
{
    public class QuestionnaireModel
    {
        public string Title { get; set; }
        public Dictionary<Guid, QuestionnaireScreen> Screens { get; set; } 
    }

    public class QuestionnaireScreen
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }

        public string Title { get; set; }
        public bool IsRoster { get; set; }

        public List<Guid> Breadcrumbs { get; set; }

        public List<IComposite> Items { get; set; }
    }
}
