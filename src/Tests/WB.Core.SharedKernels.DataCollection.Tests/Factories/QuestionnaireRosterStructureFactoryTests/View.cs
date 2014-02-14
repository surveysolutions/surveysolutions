using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.SharedKernels.DataCollection.Tests.Views
{
    public class View : IReadSideRepositoryEntity, IVersionedView {
        public long Version { get; set; }
    }
}