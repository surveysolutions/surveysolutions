// -----------------------------------------------------------------------
// <copyright file="ClientEventSyncTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Core.CAPI.Synchronization;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace Core.CAPI.Tests.Synchronization
{
    using System.Collections.Generic;

    using Core.CAPI.Views.ExporStatistics;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class ExportStatisticsFactoryTests
    {
        [Test]
        public void Load_KeyListIsEmpty_EmptyListReturned()
        {
            var store = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            Guid eventSourceId = Guid.NewGuid();
            var questionnaireList = new[]
                                        {
                                            new CompleteQuestionnaireBrowseItem(new CompleteQuestionnaireDocument()
                                                                                    {
                                                                                        Responsible = new UserLight(new Guid(), "User"),
                                                                                        PublicKey = eventSourceId,
                                                                                        Status = SurveyStatus.Initial
                                                                                    })
                                        }.AsQueryable();
            store.Setup(
                x =>
                x.Query()).Returns(questionnaireList);

            var factory = new ExportStatisticsFactory(store.Object);

            var target = factory.Load(new ExporStatisticsInputModel(new List<Guid>()));
            
            Assert.AreEqual(target.Items.Count(), 0);
           
        }

        [Test]
        public void Load_KeyListNotEmptyButStorageIsEmpty_EmptyListReturned()
        {
            var store = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            var questionnaireList = new CompleteQuestionnaireBrowseItem[0].AsQueryable();
            store.Setup(x => x.Query()).Returns(questionnaireList);

            var factory = new ExportStatisticsFactory(store.Object);

            var target = factory.Load(new ExporStatisticsInputModel(new List<Guid>()));

            Assert.AreEqual(target.Items.Count(), 0);

        }

        [Test]
        public void Load_KeyListNotEmpty_NotEmptyListReturned()
        {
            var store = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            Guid eventSourceId = Guid.NewGuid();
            var questionnaireList = new[]
                                        {
                                            new CompleteQuestionnaireBrowseItem(new CompleteQuestionnaireDocument
                                                                                    {
                                                                                        Responsible = new UserLight(new Guid(), "User"),
                                                                                        PublicKey = eventSourceId,
                                                                                        Status = SurveyStatus.Initial
                                                                                    })
                                        }.AsQueryable();
            store.Setup(
                x =>
                x.Query()).Returns(questionnaireList);

            var factory = new ExportStatisticsFactory(store.Object);

            var target = factory.Load(new ExporStatisticsInputModel(new List<Guid>{ eventSourceId }));

            Assert.AreEqual(target.Items.Count(), 1);
        }
    }
}
