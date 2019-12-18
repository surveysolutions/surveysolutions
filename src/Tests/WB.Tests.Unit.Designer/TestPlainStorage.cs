using WB.Core.Infrastructure.Implementation;

namespace WB.Tests.Unit.Designer
{
    public class TestPlainStorage<T> : InMemoryPlainStorageAccessor<T> where T : class
    {
    }
}
