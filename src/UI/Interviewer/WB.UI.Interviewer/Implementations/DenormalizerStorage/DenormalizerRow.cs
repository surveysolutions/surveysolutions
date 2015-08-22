using Cirrious.MvvmCross.Plugins.Sqlite;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.UI.Interviewer.Implementations.DenormalizerStorage
{
    public abstract class DenormalizerRow : IView
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}