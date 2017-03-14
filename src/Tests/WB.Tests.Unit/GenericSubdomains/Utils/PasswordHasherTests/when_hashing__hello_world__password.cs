using Machine.Specifications;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Infrastructure.Shared.Enumerator;

namespace WB.Tests.Unit.GenericSubdomains.Utils.PasswordHasherTests
{
    internal class when_hashing__hello_world__password : PasswordHasherTestContext
    {
        Establish context = () =>
        {
            hasher = CreatePasswordHasher();
        };

        Because of = () => 
            result = hasher.Hash(password);

        It should = () =>
            result.ShouldEqual("45e08bb98ecaf19d96ca0046ae994ba7f83fa83d");

        private static PasswordHasher hasher;
        private static string password = "привет world";
        private static string result;
    }

    public class WhenHashing_Password_With_PBKDF_Algo
    {
        private DevicePasswordHasher subject;

        [OneTimeSetUp]
        public void Establish()
        {
            this.subject = new DevicePasswordHasher();
        }

        [Test]
        public void Should_Hash_And_Validate_Password()
        {
            var hash = this.subject.Hash("Hello");
            var anotherHash = this.subject.Hash("Hello");

            Assert.That(hash, Is.Not.EqualTo(anotherHash));

            Assert.True(this.subject.VerifyPassword(hash, "Hello"));
            Assert.True(this.subject.VerifyPassword(anotherHash, "Hello"));
            Assert.False(this.subject.VerifyPassword(hash, "NotHello"));
        }
    }
}