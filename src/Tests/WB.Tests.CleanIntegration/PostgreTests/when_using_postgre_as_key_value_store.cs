using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.CleanIntegration.PostgreTests
{
    [Subject(typeof (PostgreKeyValueStorage<>))]
    public class when_using_postgre_as_key_value_store
    {
        Establish context = () =>
        {
            store = new PostgreKeyValueStorage<PostgreView>(new PostgreConnectionSettings
            {
                ConnectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=Qwerty1234;Database=SuperHQ-RC-Views;"
            });

            viewId = "cfa9d80f-91ef-4267-9b6b-bb2782041994";
            store.Store(new PostgreView{IntField = 4, Text = "это текст на русском языке Ъё"}, viewId);
        };

        Because of = () => { storedEntity = store.GetById(viewId); };

        It should_read_stored_entity = () => storedEntity.ShouldNotBeNull();

        static PostgreKeyValueStorage<PostgreView> store;
        static string viewId;
        static PostgreView storedEntity;
    }

    public class PostgreView : IReadSideRepositoryEntity
    {
        public string Text { get; set; }

        public int IntField { get; set; }
    }
}

