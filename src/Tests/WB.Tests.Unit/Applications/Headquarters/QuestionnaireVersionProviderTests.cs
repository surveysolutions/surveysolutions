using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Applications.Headquarters
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireVersionProvider))]
    public class QuestionnaireVersionProviderTests
    {
        [Test]
        public void when_getting_previous_version_should_skip_deleted_versions()
        {
            var questionnaireId = Guid.NewGuid();
            var versions = new[]
            {
                new QuestionnaireBrowseItem { QuestionnaireId = questionnaireId, Version = 1 },
                new QuestionnaireBrowseItem { QuestionnaireId = questionnaireId, Version = 2, IsDeleted = true },
            }.AsQueryable();

            var storage = new Mock<IPlainStorageAccessor<QuestionnaireBrowseItem>>();
            storage
                .Setup(x => x.Query(It.IsAny<Func<IQueryable<QuestionnaireBrowseItem>, List<long>>>()))
                .Returns((Func<IQueryable<QuestionnaireBrowseItem>, List<long>> query) => query(versions));

            var provider = new QuestionnaireVersionProvider(storage.Object);

            var previousVersion = provider.GetPreviousVersion(questionnaireId, 3);

            Assert.That(previousVersion, Is.EqualTo(1));
        }
    }
}
