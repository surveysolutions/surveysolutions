using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.Enumerator
{
    [TestOf(typeof(SqlitePlainStorage<,>))]
    public class SqliteStorageTests
    {
        [Test]
        public void should_be_able_to_store_and_read_entity()
        {
            SqliteInmemoryStorage<InterviewView> interviews = new SqliteInmemoryStorage<InterviewView>();

            Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            interviews.Store(Create.Entity.InterviewView(interviewId: interviewId));

            var interviewView = interviews.GetById(interviewId.FormatGuid());
            Assert.That(interviewView, Is.Not.Null);
        }

        [Test]
        public void when_reading_non_existing_entiry_should_return_null()
        {
            SqliteInmemoryStorage<InterviewView> interviews = new SqliteInmemoryStorage<InterviewView>();
            Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var interviewView = interviews.GetById(interviewId.FormatGuid());
            Assert.That(interviewView, Is.Null);
        }
    }
}
