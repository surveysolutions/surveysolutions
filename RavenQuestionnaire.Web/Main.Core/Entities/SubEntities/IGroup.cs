using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities
{
    public interface IGroup : IComposite, IConditional
    {
        [Obsolete("Left for backward compatibility with Saint Lucia and Marocco started before 11/8/2013.")]
        Propagate Propagated { get; set; }

        string Title { get; set; }

        string Description { get; set; }

        string VariableName { get; set; }

        bool IsRoster { get; }

        Guid? RosterSizeQuestionId { get; }

        RosterSizeSourceType RosterSizeSource { get; }

        string[] RosterFixedTitles { get; }

        Guid? RosterTitleQuestionId { get; }
    }
}