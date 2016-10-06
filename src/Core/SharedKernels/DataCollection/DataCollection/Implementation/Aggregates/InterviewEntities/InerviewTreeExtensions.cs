using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public static class InerviewTreeExtensions
    {
        public static IReadOnlyCollection<InterviewTreeNodeDiff> FindDiff(this InterviewTree sourceTree, InterviewTree changedTree)
        {
            var sourceNodes = sourceTree.Sections.SelectMany(x => x.Children).TreeToEnumerable(x => x.Children).ToList();
            var changedNodes = changedTree.Sections.SelectMany(x => x.Children).TreeToEnumerable(x => x.Children).ToList();

            var leftOuterJoin = from source in sourceNodes
                join changed in changedNodes
                    on source.Identity equals changed.Identity
                    into temp
                       from changed in temp.DefaultIfEmpty()
                       select new InterviewTreeNodeDiff() {SourceNode = source, ChangedNode = changed};

            var rightOuterJoin = from changed in changedNodes
                join source in sourceNodes
                    on changed.Identity equals source.Identity
                    into temp
                from source in temp.DefaultIfEmpty()
                select new InterviewTreeNodeDiff() {SourceNode = source, ChangedNode = changed};

            var fullOuterJoin = leftOuterJoin.Concat(rightOuterJoin);

            return fullOuterJoin
                .DistinctBy(x => new {sourceIdentity = x.SourceNode?.Identity, changedIdentity = x.ChangedNode?.Identity})
                .ToReadOnlyCollection();
        }
    }
}