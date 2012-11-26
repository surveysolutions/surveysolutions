// -----------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireExportViewFactoryTest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.View.CompleteQuestionnaire;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;

namespace RavenQuestionnaire.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class CompleteQuestionnaireExportViewFactoryTest
    {
        protected CompleteQuestionnaireExportViewFactoryTestable Target { get; set; }
        protected Mock<IDenormalizerStorage<CompleteQuestionnaireStoreDocument>> DetailsView;

        protected Mock<IDenormalizerStorage<QuestionnaireDocument>> TemplateStore;

        [SetUp]
        public void CreateObjects()
        {
            this.DetailsView = new Mock<IDenormalizerStorage<CompleteQuestionnaireStoreDocument>>();
            this.TemplateStore = new Mock<IDenormalizerStorage<QuestionnaireDocument>>();
            this.Target = new CompleteQuestionnaireExportViewFactoryTestable(this.DetailsView.Object,
                                                                             this.TemplateStore.Object);
        }

        [Test]
        public void Load_TemplateIsAbsent_EmptyResultISReturned()
        {
            var result = this.Target.Load(new CompleteQuestionnaireExportInputModel(new Guid[]{Guid.NewGuid()},Guid.NewGuid() ));
            Assert.IsTrue(!result.Items.Any());
            Assert.IsTrue(!result.Header.Any());
            Assert.IsTrue(!result.SubPropagatebleGroups.Any());
        }

        [Test]
        public void Load_QuestionnairiesIsAbsent_EmptyResultISReturned()
        {
            this.TemplateStore.Setup(x => x.GetByGuid(It.IsAny<Guid>())).Returns(new QuestionnaireDocument());
            var result = this.Target.Load(new CompleteQuestionnaireExportInputModel(Enumerable.Empty<Guid>(),Guid.NewGuid()));
            Assert.IsTrue(!result.Items.Any());
            Assert.IsTrue(!result.Header.Any());
            Assert.IsTrue(!result.SubPropagatebleGroups.Any());
        }
        [Test]
        public void BuildHeader_GroupIsEmpty_EmptyHeader()
        {
            var group = new Group("some group");
            var result = this.Target.BuildHeaderTestable(group);
            Assert.IsTrue(result.Count == 0);
        }
        [Test]
        public void BuildHeader_GroupWithQuestion_QuestionISAddedToHeader()
        {
            var group = new Group("some group");
            var questionKey = Guid.NewGuid();
            group.Add(new SingleQuestion(questionKey, "questionText"), null);
            var result = this.Target.BuildHeaderTestable(group);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Value.Title == "questionText" && result.First().Key == questionKey);
        }
        [Test]
        public void BuildHeader_GroupWithQuestionInsideSubGroup_QuestionISAddedToHeader()
        {
            var group = new Group("some group");
            var subGroup = new Group("subgroup");
            group.Add(subGroup, null);
            var questionKey = Guid.NewGuid();
            subGroup.Add(new SingleQuestion(questionKey, "questionText"), null);
            var result = this.Target.BuildHeaderTestable(group);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Value.Title == "questionText" && result.First().Key == questionKey);
        }
        [Test]
        public void BuildHeader_PropagatedGroupWithQuestionInsideSubGroup_QuestionISAddedToHeader()
        {
            var group = new Group("some group");
            var subGroup = new Group("subgroup"){ Propagated = Propagate.Propagated};
            group.Add(subGroup, null);
            var questionKey = Guid.NewGuid();
            subGroup.Add(new SingleQuestion(questionKey, "questionText"), null);
            var result = this.Target.BuildHeaderTestable(group);
            Assert.IsTrue(result.Count ==0);
           
        }
        protected class CompleteQuestionnaireExportViewFactoryTestable : CompleteQuestionnaireExportViewFactory
        {
            public CompleteQuestionnaireExportViewFactoryTestable(
                IDenormalizerStorage<CompleteQuestionnaireStoreDocument> documentSession,
                IDenormalizerStorage<QuestionnaireDocument> templateStore)
                : base(documentSession,templateStore)
            {
            }

            public Dictionary<Guid, HeaderItem> BuildHeaderTestable(IGroup template)
            {
                return base.BuildHeader(template, new List<Guid>());
            }
        }
    }
}
