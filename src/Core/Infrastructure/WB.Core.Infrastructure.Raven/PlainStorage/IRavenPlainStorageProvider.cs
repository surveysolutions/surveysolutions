using Raven.Client;

namespace WB.Core.Infrastructure.Raven.PlainStorage
{
    public interface IRavenPlainStorageProvider
    {
        IDocumentStore GetDocumentStore();
    }
}