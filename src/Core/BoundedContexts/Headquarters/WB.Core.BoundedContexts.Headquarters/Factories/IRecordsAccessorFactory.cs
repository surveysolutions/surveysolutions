using System.IO;
using WB.Core.BoundedContexts.Headquarters.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    public interface IRecordsAccessorFactory
    {
        IRecordsAccessor CreateRecordsAccessor(Stream sampleStream, string delimiter);
    }
}
