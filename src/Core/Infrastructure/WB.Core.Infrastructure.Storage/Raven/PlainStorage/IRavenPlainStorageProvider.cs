using Raven.Client;

namespace WB.Core.Infrastructure.Storage.Raven.PlainStorage
{
    public interface IRavenPlainStorageProvider
    {
        IDocumentStore GetDocumentStore();
    }
}