namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Utility;
    using Main.Core.View.Group;

    /// <summary>
    /// The complete group headers.
    /// </summary>
    public class DetailsMenuItem
    {
        public bool Enabled { get; set; }

        public string GroupText { get; set; }

        public Counter Totals { get; set; }

        public string Description { get; set; }

        public Propagate Propagated { get; set; }

        public int Level { get; set; }

        public ScreenKey Key { get; set; }

    }
}