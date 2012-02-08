using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.Observers
{
    public class CompositeInfo
    {
        public CompositeInfo()
        {
        }

        public IComposite Document { get; set; }
        public IComposite Target { get; set; }
        public Actions Action { get; set; }
    }

    public enum Actions
    {
        Add,
        Remove
    }
}
