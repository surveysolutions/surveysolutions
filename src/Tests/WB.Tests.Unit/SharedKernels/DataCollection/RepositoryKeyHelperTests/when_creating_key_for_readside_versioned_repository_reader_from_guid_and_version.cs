using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Tests.Unit.SharedKernels.DataCollection.RepositoryKeyHelperTests
{
    internal class when_creating_key_for_readside_versioned_repository_reader_from_guid_and_version
    {
        [NUnit.Framework.OneTimeSetUp] public void context () { BecauseOf();}

        public void BecauseOf() =>
            key = RepositoryKeysHelper.GetVersionedKey(Guid.Parse("11111111111111111111111111111111"), 1);

        [NUnit.Framework.Test] public void should_create_key_11111111111111111111111111111111_1 () =>
            key.Should().Be("11111111111111111111111111111111-1");

        private static string key;
    }
}
