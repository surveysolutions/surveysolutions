using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
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

            Assert.That(subject.VerifyPassword(hash, "Hello"), Is.EqualTo(PasswordVerificationResult.Success));
            Assert.That(subject.VerifyPassword(anotherHash, "Hello"), Is.EqualTo(PasswordVerificationResult.Success));
            Assert.That(subject.VerifyPassword(hash, "NotHello"), Is.EqualTo(PasswordVerificationResult.Failed));
        }

        [Test]
        public void Should_work_has_different_hashes_for_Hello()
        {
            var subject = new DevicePasswordHasher();

            var hash = subject.Hash("Hello");

            Assert.That(subject.VerifyPassword(hash, "Hello"), Is.EqualTo(PasswordVerificationResult.Success));
            Assert.That(subject.VerifyPassword(hash, "HElLo"), Is.EqualTo(PasswordVerificationResult.Failed));
            Assert.That(subject.VerifyPassword(hash, "Heeello"), Is.EqualTo(PasswordVerificationResult.Failed));
            Assert.That(subject.VerifyPassword(hash, "Helllllo"), Is.EqualTo(PasswordVerificationResult.Failed));
        }

        [Test]
        public void should_require_rehash_if_used_older_hash()
        {
            var subject = new DevicePasswordHasher();
            const string password = "Hellloo";
            var hash = Pre11662HashingAlgo(password);

            var verificationResult = subject.VerifyPassword(hash, password);

            Assert.That(verificationResult, Is.EqualTo(PasswordVerificationResult.SuccessRehashNeeded));
        }
        
        [Test]
        public void should_fail_for_improper_hash_structure()
        {
            var subject = new DevicePasswordHasher();
            const string password = "Hellloo";
            var hash = "aGVsbG8=:" + Pre11662HashingAlgo(password);

            var verificationResult = subject.VerifyPassword(hash, password);

            Assert.That(verificationResult, Is.EqualTo(PasswordVerificationResult.Failed));
        }

        [Test]
        public void should_fail_for_wrong_password_using_older_hash()
        {
            var subject = new DevicePasswordHasher();
            const string password = "Hellloo";
            var hash = Pre11662HashingAlgo(password);

            var verificationResult = subject.VerifyPassword(hash, password + "Wrong");

            Assert.That(verificationResult, Is.EqualTo(PasswordVerificationResult.Failed));
        }

        private string Pre11662HashingAlgo(string password)
        {
            var rng = new RNGCryptoServiceProvider();
            var data = Encoding.UTF8.GetBytes(password);
            var saltBuffer = new byte[20];
            rng.GetBytes(saltBuffer);
            var hash = SHA512.Create().ComputeHash(data.Union(saltBuffer).ToArray());
            return Convert.ToBase64String(saltBuffer) + ":" + Convert.ToBase64String(hash);
        }
    }
}
