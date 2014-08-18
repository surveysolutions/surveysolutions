using Raven.Client;

namespace WB.Core.Infrastructure.Raven.Raven.PlainStorage
{
    public interface IRavenPlainStorageProvider
    {
        IDocumentStore GetDocumentStore();
    }
}