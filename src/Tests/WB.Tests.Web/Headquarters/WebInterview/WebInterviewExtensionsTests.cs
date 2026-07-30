using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using WB.UI.Headquarters.Code.WebInterview;

namespace WB.Tests.Web.Headquarters.WebInterview
{
    [TestFixture]
    [TestOf(typeof(WebInterviewExtensions))]
    public class WebInterviewExtensionsTests
    {
        private const string TestInterviewId = "a11fa92b-c114-4d24-b3e8-5ba38ff24d53";

        [Test]
        public void IsPasswordVerifiedForInterview_WhenSessionHasNoEntries_ShouldReturnFalse()
        {
            var session = new MockSession();

            var result = session.IsPasswordVerifiedForInterview(TestInterviewId);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsPasswordVerifiedForInterview_WhenInterviewIsInVerifiedList_ShouldReturnTrue()
        {
            var session = new MockSession();
            session.Set(WebInterviewExtensions.PasswordVerifiedKey,
                SerializeList(new List<string> { TestInterviewId }));

            var result = session.IsPasswordVerifiedForInterview(TestInterviewId);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsPasswordVerifiedForInterview_WhenDifferentInterviewIsVerified_ShouldReturnFalse()
        {
            const string otherInterviewId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
            var session = new MockSession();
            session.Set(WebInterviewExtensions.PasswordVerifiedKey,
                SerializeList(new List<string> { otherInterviewId }));

            var result = session.IsPasswordVerifiedForInterview(TestInterviewId);

            Assert.That(result, Is.False);
        }

        [Test]
        public void PasswordVerifiedKey_ShouldBeConsistentStringValue()
        {
            Assert.That(WebInterviewExtensions.PasswordVerifiedKey, Is.EqualTo("PasswordVerifiedKey"));
        }

        private static byte[] SerializeList(List<string> list)
        {
            // Mimics the ISession.Set<T> behavior used via Microsoft.AspNetCore.Http.SessionExtensions
            var json = System.Text.Json.JsonSerializer.Serialize(list);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        private class MockSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new();

            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
            public void Set(string key, byte[] value) => _store[key] = value;
            public void Remove(string key) => _store.Remove(key);
            public void Clear() => _store.Clear();
            public bool IsAvailable => true;
            public string Id => "test-session";
            public IEnumerable<string> Keys => _store.Keys;
        }
    }
}
