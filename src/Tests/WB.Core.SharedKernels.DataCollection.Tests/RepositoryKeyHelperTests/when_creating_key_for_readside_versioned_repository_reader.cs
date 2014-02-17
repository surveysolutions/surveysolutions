﻿using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace Main.Core.Tests.Utils
{
    public class when_creating_key_for_readside_versioned_repository_reader
    {
        Establish context = () => {};

        Because of = () => 
            key = RepositoryKeysHelper.GetVersionedKey("11111111111111111111111111111111", 1);

        It should_create_key_11111111111111111111111111111111_1 = () =>
            key.ShouldEqual("11111111111111111111111111111111-1");

        private static string key;
    }
}
