using WB.UI.WebTester.Services;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Web.TestFactories
{
    public class StorageFactory
    {
        public InMemoryCacheStorage<MultimediaFile, string> MediaStorage()
        {
            return new InMemoryCacheStorage<MultimediaFile, string>();
        }
    }
}
