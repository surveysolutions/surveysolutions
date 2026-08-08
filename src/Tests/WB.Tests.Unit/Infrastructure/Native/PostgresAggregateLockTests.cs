using System;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Tests.Unit.Infrastructure.Native
{
    [TestOf(typeof(PostgresAggregateLock))]
    public class PostgresAggregateLockTests
    {
        [Test]
        public void when_getting_advisory_lock_key_for_valid_guid_should_return_stable_value()
        {
            var guid = "12345678-1234-1234-1234-123456789abc";

            var key1 = PostgresAggregateLock.GetAdvisoryLockKey(guid);
            var key2 = PostgresAggregateLock.GetAdvisoryLockKey(guid);

            Assert.That(key1, Is.EqualTo(key2));
        }

        [Test]
        public void when_getting_advisory_lock_key_for_same_guid_with_different_casing_should_return_same_value()
        {
            var lowerGuid = "12345678-abcd-ef01-2345-6789abcdef01";
            var upperGuid = "12345678-ABCD-EF01-2345-6789ABCDEF01";

            var lowerKey = PostgresAggregateLock.GetAdvisoryLockKey(lowerGuid);
            var upperKey = PostgresAggregateLock.GetAdvisoryLockKey(upperGuid);

            Assert.That(lowerKey, Is.EqualTo(upperKey));
        }

        [Test]
        public void when_getting_advisory_lock_key_for_different_guids_should_return_different_values()
        {
            // Use fixed GUIDs differing only in one byte to verify XOR sensitivity
            var guid1 = "12345678-1234-1234-1234-000000000001";
            var guid2 = "12345678-1234-1234-1234-000000000002";

            var key1 = PostgresAggregateLock.GetAdvisoryLockKey(guid1);
            var key2 = PostgresAggregateLock.GetAdvisoryLockKey(guid2);

            Assert.That(key1, Is.Not.EqualTo(key2));
        }

        [Test]
        public void when_getting_advisory_lock_key_for_non_guid_string_should_return_non_zero_value()
        {
            var nonGuid = "some-non-guid-string";

            var key = PostgresAggregateLock.GetAdvisoryLockKey(nonGuid);

            Assert.That(key, Is.Not.EqualTo(0));
        }

        [Test]
        public void when_getting_advisory_lock_key_for_non_guid_string_should_return_stable_value()
        {
            var nonGuid = "some-non-guid-string";

            var key1 = PostgresAggregateLock.GetAdvisoryLockKey(nonGuid);
            var key2 = PostgresAggregateLock.GetAdvisoryLockKey(nonGuid);

            Assert.That(key1, Is.EqualTo(key2));
        }

        [Test]
        public void when_getting_advisory_lock_key_for_different_non_guid_strings_should_return_different_values()
        {
            var key1 = PostgresAggregateLock.GetAdvisoryLockKey("aggregate-type-a");
            var key2 = PostgresAggregateLock.GetAdvisoryLockKey("aggregate-type-b");

            Assert.That(key1, Is.Not.EqualTo(key2));
        }

        [Test]
        public void when_getting_advisory_lock_key_for_all_guids_should_fit_in_int64()
        {
            for (int i = 0; i < 100; i++)
            {
                var guid = Guid.NewGuid().ToString();
                var key = PostgresAggregateLock.GetAdvisoryLockKey(guid);

                // Simply verify that no overflow exception is thrown and the key is a valid long
                Assert.That(key, Is.InstanceOf<long>());
            }
        }
    }
}
