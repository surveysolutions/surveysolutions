#nullable enable
using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireCompilationVersionService))]
    public class QuestionnaireCompilationVersionServiceTests
    {
        private static QuestionnaireCompilationVersionService CreateService()
            => new QuestionnaireCompilationVersionService(Create.InMemoryDbContext());

        [Test]
        public void when_added_should_be_returned_by_id_and_list()
        {
            var service = CreateService();
            var id = Guid.NewGuid();

            service.Add(new QuestionnaireCompilationVersion { QuestionnaireId = id, Version = 5, Description = "d" });

            service.GetById(id)!.Version.Should().Be(5);
            service.GetCompilationVersions().Select(x => x.QuestionnaireId).Should().Contain(id);
        }

        [Test]
        public void when_not_found_should_return_null()
        {
            CreateService().GetById(Guid.NewGuid()).Should().BeNull();
        }

        [Test]
        public void when_updated_should_persist_changes()
        {
            var service = CreateService();
            var id = Guid.NewGuid();
            var version = new QuestionnaireCompilationVersion { QuestionnaireId = id, Version = 1 };
            service.Add(version);

            version.Version = 9;
            service.Update(version);

            service.GetById(id)!.Version.Should().Be(9);
        }

        [Test]
        public void when_removed_should_no_longer_exist()
        {
            var service = CreateService();
            var id = Guid.NewGuid();
            service.Add(new QuestionnaireCompilationVersion { QuestionnaireId = id, Version = 1 });

            service.Remove(id);

            service.GetById(id).Should().BeNull();
        }

        [Test]
        public void when_removing_missing_should_not_throw()
        {
            var service = CreateService();

            Assert.DoesNotThrow(() => service.Remove(Guid.NewGuid()));
        }
    }
}

