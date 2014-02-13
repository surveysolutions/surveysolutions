using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Utility;

namespace Main.Core.Tests.Utils
{
    public class when_creating_key_for_readside_versioned_repository_reader
    {
        Establish context = () => {};

        Because of = () => 
            key = RepositoryKeysHelper.GetVersionedKey("11111111111111111111111111111111", 1);

        private It should_create_key_11111111111111111111111111111111_1 = () =>
            key.ShouldEqual("11111111111111111111111111111111-1");

        private static string key;
    }
}
