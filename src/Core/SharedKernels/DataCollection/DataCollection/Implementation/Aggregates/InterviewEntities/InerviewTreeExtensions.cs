using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public static class InerviewTreeExtensions
    {
        public static IReadOnlyCollection<InterviewTreeNodeDiff> FindDiff(this InterviewTree sourceTree, InterviewTree changedTree)
        {
            var diff = from source in sourceTree.Sections.SelectMany(x => x.Children).TreeToEnumerable(x => x.Children)
                join changed in changedTree.Sections.SelectMany(x => x.Children).TreeToEnumerable(x => x.Children)
                    on source.Identity equals changed.Identity
                select new InterviewTreeNodeDiff() {SourceNode = source, ChangedNode = changed};

            return diff.ToReadOnlyCollection();
        }
    }
}