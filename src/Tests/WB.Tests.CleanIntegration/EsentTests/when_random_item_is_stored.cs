using System;
using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Esent.Implementation;

namespace WB.Tests.CleanIntegration.EsentTests
{
    [Subject(typeof (EsentKeyValueStorage<>))]
    internal class when_random_item_is_stored : with_esent_store<TestStoredEntity>
    {
        Establish context = () =>
        {
            transientEntity = new TestStoredEntity
            {
                Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"),
                IntegerProperty = 5,
                StringProperty = "Some test string"
            };
            storage.Store(transientEntity, itemId);
        };

        Because of = () => persistedEntity = storage.GetById(itemId);

        It should_read_item_from_store = () =>
        {
            persistedEntity.ShouldNotBeNull();
            persistedEntity.Id.ShouldEqual(transientEntity.Id);
            persistedEntity.IntegerProperty.ShouldEqual(transientEntity.IntegerProperty);
            persistedEntity.StringProperty.ShouldEqual(transientEntity.StringProperty);
        };

        const string itemId = "id";
        static TestStoredEntity persistedEntity;
        static TestStoredEntity transientEntity;
    }
}

