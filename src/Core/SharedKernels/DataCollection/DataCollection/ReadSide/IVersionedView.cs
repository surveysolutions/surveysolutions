using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.ReadSide
{
    public interface IVersionedView : IView
    {
        long Version { get; set; }
    }
}
