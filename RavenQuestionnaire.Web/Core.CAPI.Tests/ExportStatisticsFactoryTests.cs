using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Core.CAPI.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Core.CAPI.Views.ExporStatistics;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ExportStatisticsFactoryTests
    {
        [Test]
        public void Load_KeyListIsEmpty_EmptyListReturned()
        {
            var store = new Mock<IQueryableReadSideRepositoryReader<CompleteQuestionnaireBrowseItem>>();
            Guid eventSourceId = Guid.NewGuid();
            IQueryable<CompleteQuestionnaireBrowseItem> questionnaireList =
                new[]
                    {
                        new CompleteQuestionnaireBrowseItem(
                            new CompleteQuestionnaireDocument
                                {
                                    Responsible = new UserLight(new Guid(), "User"), 
                                    PublicKey = eventSourceId, 
                                    Status = SurveyStatus.Initial
                                })
                    }.AsQueryable();

            store.Setup(x => x.Query(It.IsAny<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<CompleteQuestionnaireBrowseItem>>>()))
                .Returns<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<CompleteQuestionnaireBrowseItem>>>(query => query.Invoke(questionnaireList));

            var factory = new ExportStatisticsFactory(store.Object);

            ExportStatisticsView target = factory.Load(new ExporStatisticsInputModel(new List<Guid>()));

            Assert.AreEqual(target.Items.Count(), 0);
        }

        [Test]
        public void Load_KeyListNotEmptyButStorageIsEmpty_EmptyListReturned()
        {
            var store = new Mock<IQueryableReadSideRepositoryReader<CompleteQuestionnaireBrowseItem>>();
            IQueryable<CompleteQuestionnaireBrowseItem> questionnaireList =
                new CompleteQuestionnaireBrowseItem[0].AsQueryable();

            store.Setup(x => x.Query(It.IsAny<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<CompleteQuestionnaireBrowseItem>>>()))
                .Returns<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<CompleteQuestionnaireBrowseItem>>>(query => query.Invoke(questionnaireList));

            var factory = new ExportStatisticsFactory(store.Object);

            ExportStatisticsView target = factory.Load(new ExporStatisticsInputModel(new List<Guid>()));

            Assert.AreEqual(target.Items.Count(), 0);
        }

        [Test]
        public void Load_KeyListNotEmpty_NotEmptyListReturned()
        {
            var store = new Mock<IQueryableReadSideRepositoryReader<CompleteQuestionnaireBrowseItem>>();
            Guid eventSourceId = Guid.NewGuid();
            IQueryable<CompleteQuestionnaireBrowseItem> questionnaireList =
                new[]
                    {
                        new CompleteQuestionnaireBrowseItem(
                            new CompleteQuestionnaireDocument
                                {
                                    Responsible = new UserLight(new Guid(), "User"), 
                                    PublicKey = eventSourceId, 
                                    Status = SurveyStatus.Initial
                                })
                    }.AsQueryable();

            store.Setup(x => x.Query(It.IsAny<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<CompleteQuestionnaireBrowseItem>>>()))
                .Returns<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<CompleteQuestionnaireBrowseItem>>>(query => query.Invoke(questionnaireList));

            var factory = new ExportStatisticsFactory(store.Object);

            ExportStatisticsView target = factory.Load(new ExporStatisticsInputModel(new List<Guid> { eventSourceId }));

            Assert.AreEqual(target.Items.Count(), 1);
        }
    }
}