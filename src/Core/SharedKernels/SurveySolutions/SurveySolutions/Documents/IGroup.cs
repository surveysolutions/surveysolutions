using System;
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

        [Obsolete]
        string[] RosterFixedTitles { get; }
        
        Tuple<decimal, string>[] FixedRosterTitles { get; }

        Guid? RosterTitleQuestionId { get; }
    }
}