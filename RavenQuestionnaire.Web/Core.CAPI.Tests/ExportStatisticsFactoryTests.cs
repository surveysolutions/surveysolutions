// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExportStatisticsFactoryTests.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
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

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class ExportStatisticsFactoryTests
    {
        #region Public Methods and Operators

        /// <summary>
        /// The load_ key list is empty_ empty list returned.
        /// </summary>
        [Test]
        public void Load_KeyListIsEmpty_EmptyListReturned()
        {
            var store = new Mock<IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
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
            store.Setup(x => x.Query()).Returns(questionnaireList);

            var factory = new ExportStatisticsFactory(store.Object);

            ExportStatisticsView target = factory.Load(new ExporStatisticsInputModel(new List<Guid>()));

            Assert.AreEqual(target.Items.Count(), 0);
        }

        /// <summary>
        /// The load_ key list not empty but storage is empty_ empty list returned.
        /// </summary>
        [Test]
        public void Load_KeyListNotEmptyButStorageIsEmpty_EmptyListReturned()
        {
            var store = new Mock<IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            IQueryable<CompleteQuestionnaireBrowseItem> questionnaireList =
                new CompleteQuestionnaireBrowseItem[0].AsQueryable();
            store.Setup(x => x.Query()).Returns(questionnaireList);

            var factory = new ExportStatisticsFactory(store.Object);

            ExportStatisticsView target = factory.Load(new ExporStatisticsInputModel(new List<Guid>()));

            Assert.AreEqual(target.Items.Count(), 0);
        }

        /// <summary>
        /// The load_ key list not empty_ not empty list returned.
        /// </summary>
        [Test]
        public void Load_KeyListNotEmpty_NotEmptyListReturned()
        {
            var store = new Mock<IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
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
            store.Setup(x => x.Query()).Returns(questionnaireList);

            var factory = new ExportStatisticsFactory(store.Object);

            ExportStatisticsView target = factory.Load(new ExporStatisticsInputModel(new List<Guid> { eventSourceId }));

            Assert.AreEqual(target.Items.Count(), 1);
        }

        #endregion
    }
}