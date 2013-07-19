using Main.Core.Entities;

namespace Core.Supervisor.Views.Summary
{
    using Main.Core.Entities.SubEntities;

    public class SummaryViewItem
    {
        public int Unassigned { get; set; }

        public int Approved { get; set; }

        public int Completed { get; set; }

        public int Error { get; set; }

        public UserLight User { get; set; }

        public TemplateLight Template { get; set; }

        public int Initial { get; set; }

        public int Redo { get; set; }

        public int Total { get; set; }
    }
}