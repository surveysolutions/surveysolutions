using System.IO;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.SampleRecordsAccessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    internal class CsvRecordsAccessorFactory : IRecordsAccessorFactory
    {
        public IRecordsAccessor CreateRecordsAccessor(Stream sampleStream, string delimiter)
        {
            return new CsvRecordsAccessor(sampleStream, delimiter);
        }
    }
}
