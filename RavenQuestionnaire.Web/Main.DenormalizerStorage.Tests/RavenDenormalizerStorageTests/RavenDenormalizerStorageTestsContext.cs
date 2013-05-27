using System.Collections.Generic;
using System.Text;

using Raven.Client.Document;

using WB.Core.Infrastructure;

namespace Main.DenormalizerStorage.Tests.RavenDenormalizerStorageTests
{
    internal class RavenDenormalizerStorageTestsContext
    {
        internal static RavenDenormalizerStorage<TView> CreateRavenDenormalizerStorage<TView>(
            DocumentStore ravenStore = null, IReadLayerStatusService readLayerStatusService = null)
            where TView : class
        {
            return new RavenDenormalizerStorage<TView>(
                ravenStore ?? new DocumentStore());
        }
    }
}
