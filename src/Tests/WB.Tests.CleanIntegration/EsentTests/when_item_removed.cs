using System;
using Machine.Specifications;

namespace WB.Tests.CleanIntegration.EsentTests
{
    internal class when_item_removed : with_esent_store<TestStoredEntity>
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

        Because of = () => storage.Remove(itemId);

        It should_be_removed_from_store = () => storage.GetById(itemId).ShouldBeNull();

        const string itemId = "id";
        static TestStoredEntity transientEntity;
    }
}

