using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.UI.Shared.Enumerator.Services;

namespace WB.Tests.Unit.GenericSubdomains.Utils.PasswordHasherTests
{
    internal class when_hashing__hello_world__password : PasswordHasherTestContext
    {
        [Test]
        public void should_hash()
        {
            PasswordHasher hasher = CreatePasswordHasher();
            string result = hasher.Hash("привет world");
            Assert.That(result, Is.EqualTo("45e08bb98ecaf19d96ca0046ae994ba7f83fa83d"));
        }
    }

    public class WhenHashing_Password_With_PBKDF_Algo
    {
        [Test]
        public void Should_Hash_And_Validate_Password()
        {
            var subject = new DevicePasswordHasher();

            var hash = subject.Hash("Hello");
            var anotherHash = subject.Hash("Hello");

            Assert.That(hash, Is.Not.EqualTo(anotherHash));

            Assert.True(subject.VerifyPassword(hash, "Hello"));
            Assert.True(subject.VerifyPassword(anotherHash, "Hello"));
            Assert.False(subject.VerifyPassword(hash, "NotHello"));
        }

        [Test]
        public void Should_work_has_different_hashes_for_Hello()
        {
            var subject = new DevicePasswordHasher();

            var hash = subject.Hash("Hello");

            Assert.True(subject.VerifyPassword(hash, "Hello"));
            Assert.False(subject.VerifyPassword(hash, "HHHeeellllooo"));
            Assert.False(subject.VerifyPassword(hash, "NotHello"));
        }
    }
}
