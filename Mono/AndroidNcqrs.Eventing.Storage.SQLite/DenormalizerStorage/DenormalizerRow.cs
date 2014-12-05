using Cirrious.MvvmCross.Plugins.Sqlite;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage
{
    public abstract class DenormalizerRow : IView
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}