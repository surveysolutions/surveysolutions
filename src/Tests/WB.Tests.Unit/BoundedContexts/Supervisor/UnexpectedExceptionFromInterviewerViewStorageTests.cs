using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Supervisor
{
    [TestOf(typeof(SqliteInmemoryStorage<UnexpectedExceptionFromInterviewerView, int?>))]
    public class UnexpectedExceptionFromInterviewerViewStorageTests
    {
        [Test]
        public void should_store_multiple_nodes()
        {
            var storage = new SqliteInmemoryStorage<UnexpectedExceptionFromInterviewerView, int?>();
            for (int i = 0; i < 10; i++)
            {
                storage.Store(new UnexpectedExceptionFromInterviewerView
                {
                    InterviewerId = Guid.NewGuid(),
                    Message = "msg" + i,
                    StackTrace = "stack" + i
                });
            }

            var stored = storage.LoadAll().ToList();

            Assert.That(stored.Count, Is.EqualTo(10));
            Assert.That(stored[7].Message, Is.EqualTo("msg7"));
            Assert.That(stored[7].Id, Is.EqualTo(8));
        }
    }
}
