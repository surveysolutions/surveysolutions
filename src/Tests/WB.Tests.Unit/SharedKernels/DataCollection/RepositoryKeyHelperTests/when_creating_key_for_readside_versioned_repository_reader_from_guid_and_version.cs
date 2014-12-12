using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Tests.Unit.SharedKernels.DataCollection.RepositoryKeyHelperTests
{
    public class when_creating_key_for_readside_versioned_repository_reader_from_guid_and_version
    {
        Establish context = () => { };

        Because of = () =>
            key = RepositoryKeysHelper.GetVersionedKey(Guid.Parse("11111111111111111111111111111111"), 1);

        It should_create_key_11111111111111111111111111111111_1 = () =>
            key.ShouldEqual("11111111111111111111111111111111-1");

        private static string key;
    }
}