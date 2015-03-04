using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;

namespace WB.Tests.CleanIntegration.PostgreTests
{
    [Subject(typeof(PostgreKeyValueStorage<>))]
    public class when_using_clear
    {
        Establish context = () =>
        {
            store = new PostgreKeyValueStorage<PostgreView>(new PostgreConnectionSettings
            {
                ConnectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=P@$$w0rd;Database=SuperHQ-RC-Views;"
            });

            viewId = "cfa9d80f-91ef-4267-9b6b-bb2782041994";
            store.Store(new PostgreView { IntField = 4, Text = "это текст на русском языке Ъё" }, viewId);
        };

        Because of = () => store.Clear();

        It should_read_stored_entity = () => store.GetById(viewId).ShouldBeNull();

        static PostgreKeyValueStorage<PostgreView> store;
        static string viewId;
    }
}