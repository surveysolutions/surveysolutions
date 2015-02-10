using System.Linq;
using System.Threading;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal abstract class BaseSynchronizationDenormalizer : BaseDenormalizer
    {
        protected int CalcNextSortIndex<T>(
           ref int currentSortIndex,
           IReadSideRepositoryWriter readSideRepositoryWriter,
           IQueryableReadSideRepositoryReader<T> queryableReadSideRepositoryReader) where T : class, IIndexedView
        {
            int sortIndex = 0;

            if (readSideRepositoryWriter != null && readSideRepositoryWriter.IsCacheEnabled)
            {
                sortIndex = currentSortIndex;
                Interlocked.Increment(ref currentSortIndex);
            }
            else
            {
                var query = queryableReadSideRepositoryReader.Query(_ => _.OrderByDescending(x => x.SortIndex).Select(x => x.SortIndex));
                if (query.Any())
                {
                    sortIndex = query.First() + 1;
                }
            }
            return sortIndex;
        }
    }
}