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

        protected Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>> ShortView;
        protected Mock<IDenormalizerStorage<QuestionnaireDocument>> TemplateStore;

        [SetUp]
        public void CreateObjects()
        {
            this.DetailsView = new Mock<IDenormalizerStorage<CompleteQuestionnaireStoreDocument>>();
            this.ShortView = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            this.TemplateStore = new Mock<IDenormalizerStorage<QuestionnaireDocument>>();
            this.Target = new CompleteQuestionnaireExportViewFactoryTestable(this.DetailsView.Object,
                                                                             this.ShortView.Object,
                                                                             this.TemplateStore.Object);
        }

        [Test]
        public void Load_TemplateIsAbsent_EmptyResultISReturned()
        {
            var result = this.Target.Load(new CompleteQuestionnaireExportInputModel() {QuestionnaryId = Guid.NewGuid()});
            Assert.IsTrue(!result.Items.Any());
            Assert.IsTrue(!result.Header.Any());
            Assert.IsTrue(!result.SubPropagatebleGroups.Any());
        }

        [Test]
        public void Load_QuestionnairiesIsAbsent_EmptyResultISReturned()
        {
            this.TemplateStore.Setup(x => x.GetByGuid(It.IsAny<Guid>())).Returns(new QuestionnaireDocument());
            var result = this.Target.Load(new CompleteQuestionnaireExportInputModel() {QuestionnaryId = Guid.NewGuid()});
            Assert.IsTrue(!result.Items.Any());
            Assert.IsTrue(!result.Header.Any());
            Assert.IsTrue(!result.SubPropagatebleGroups.Any());
        }
        [Test]
        public void BuildHeader_GroupIsEmpty_PublicKeyAndForeignKeyColumnsAreCreated()
        {
            var group = new Group("some group");
            var result = this.Target.BuildHeaderTestable(group);
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result.First().Value == "PublicKey");
            Assert.IsTrue(result.First().Key == group.PublicKey);
            Assert.IsTrue(result.Last().Value == "ForeignKey");
            Assert.IsTrue(result.Last().Key == Guid.Empty);
        }
        [Test]
        public void BuildHeader_GroupWithQuestion_QuestionISAddedToHeader()
        {
            var group = new Group("some group");
            var questionKey = Guid.NewGuid();
            group.Add(new SingleQuestion(questionKey, "questionText"), null);
            var result = this.Target.BuildHeaderTestable(group);
            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result.Skip(1).First().Value == "questionText" && result.Skip(1).First().Key == questionKey);
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
            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result.Skip(1).First().Value == "questionText" && result.Skip(1).First().Key == questionKey);
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
            Assert.IsTrue(result.Count ==2);
           
        }
        protected class CompleteQuestionnaireExportViewFactoryTestable : CompleteQuestionnaireExportViewFactory
        {
            public CompleteQuestionnaireExportViewFactoryTestable(
                IDenormalizerStorage<CompleteQuestionnaireStoreDocument> documentSession,
                IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentShortView,
                IDenormalizerStorage<QuestionnaireDocument> templateStore)
                : base(documentSession, documentShortView, templateStore)
            {
            }

            public Dictionary<Guid, string> BuildHeaderTestable(IGroup template)
            {
                return base.BuildHeader(template);
            }
        }
    }
}
