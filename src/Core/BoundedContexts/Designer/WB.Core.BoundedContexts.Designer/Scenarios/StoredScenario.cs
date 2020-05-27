using System;

namespace WB.Core.BoundedContexts.Designer.Scenarios
{
    public class StoredScenario
    {
        public  int Id { get; set; }

        public Guid QuestionnaireId { get; set; }

        public string Title { get; set; } = String.Empty;

        public string Steps { get; set; } = String.Empty;
    }
}
