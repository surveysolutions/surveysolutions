using System;

namespace WB.Core.BoundedContexts.Designer.Scenarios
{
    public class StoredScenario
    {
        public  int Id { get; set; }

        public Guid QuestionnaireId { get; set; }

        public string Title { get; set; }

        public string Steps { get; set; }
    }
}
