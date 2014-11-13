using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities
{
    public interface IGroup : IComposite, IConditional
    {
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