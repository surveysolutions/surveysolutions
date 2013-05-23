// -----------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireExportViewFactoryTest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.View.CompleteQuestionnaire;
using Main.Core.View.Export;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;

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
            var result =
                this.Target.Load(new CompleteQuestionnaireExportInputModel(new Guid[] {Guid.NewGuid()}, Guid.NewGuid()));
            Assert.IsTrue(result == null);
        }

        [Test]
        public void Load_QuestionnairiesIsAbsent_EmptyResultISReturned()
        {
            this.TemplateStore.Setup(x => x.GetById(It.IsAny<Guid>())).Returns(new QuestionnaireDocument());
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
            Assert.IsTrue(!result.Any());
        }
        [Test]
        public void BuildHeader_GroupWithQuestion_QuestionISAddedToHeader()
        {
            var group = new Group("some group");
            var questionKey = Guid.NewGuid();
            group.Children.Add(new SingleQuestion(questionKey, "questionText"));
            var result = this.Target.BuildHeaderTestable(group);
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().Title == "questionText" && result.First().PublicKey == questionKey);
        }
        [Test]
        public void BuildHeader_GroupWithAutoQuestion_QuestionISAddedToHeaderAndQuestionISAddedToAutoQuestionList()
        {
            var group = new Group("some group");
            var questionKey = Guid.NewGuid();
            var subOBjects = new List<Guid>();
            var autoQuestions = new List<CompleteQuestionnaireExportViewFactory.AutoQuestionWithTriggers>();
            group.Children.Add(new AutoPropagateQuestion() { QuestionText = "questionText", PublicKey = questionKey });
            var result = this.Target.BuildHeaderTestable(group, subOBjects, autoQuestions);
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().Title == "questionText" && result.First().PublicKey == questionKey);
            Assert.IsTrue(subOBjects.Count == 0);
            Assert.IsTrue(autoQuestions.Count == 1);
            Assert.IsTrue(autoQuestions[0].PublicKey == questionKey);

        }


        [Test]
        public void BuildHeader_GroupWithAutoQuestionAndPropagationGroupFromTrigger_QuestionISAddedToHeaderAndQuestionISAddedToAutoQuestionListPropagationGroupNotAdded()
        {
            var group = new Group("some group");
            var questionKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            var subOBjects = new List<Guid>();
            var autoQuestions = new List<CompleteQuestionnaireExportViewFactory.AutoQuestionWithTriggers>();
            group.Children.Add(
                new AutoPropagateQuestion() { QuestionText = "questionText", PublicKey = questionKey, Triggers = new List<Guid> { groupKey } });

            var subGroup = new Group("subgroup") { Propagated = Propagate.Propagated, PublicKey = groupKey};
            group.Children.Add(subGroup);

            var result = this.Target.BuildHeaderTestable(group, subOBjects, autoQuestions);
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().Title == "questionText" && result.First().PublicKey == questionKey);
            Assert.IsTrue(subOBjects.Count == 0);
            Assert.IsTrue(autoQuestions.Count == 1);
            Assert.IsTrue(autoQuestions[0].PublicKey == questionKey && autoQuestions[0].Triggers.Count() == 1 &&
                          autoQuestions[0].Triggers.First() == groupKey);

        }
        [Test]
        public void BuildHeader_GroupWithQuestionInsideSubGroup_QuestionISAddedToHeader()
        {
            var group = new Group("some group");
            var subGroup = new Group("subgroup");
            group.Children.Add(subGroup);
            var questionKey = Guid.NewGuid();
            subGroup.Children.Add(new SingleQuestion(questionKey, "questionText"));
            var result = this.Target.BuildHeaderTestable(group);
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().Title == "questionText" && result.First().PublicKey == questionKey);
        }
        [Test]
        public void BuildHeader_PropagatedGroupWithQuestionInsideSubGroup_QuestionISAddedToHeader()
        {
            var group = new Group("some group");
            var subGroup = new Group("subgroup"){ Propagated = Propagate.Propagated};
            group.Children.Add(subGroup);
            var questionKey = Guid.NewGuid();
            subGroup.Children.Add(new SingleQuestion(questionKey, "questionText"));
            var result = this.Target.BuildHeaderTestable(group);
            Assert.IsTrue(!result.Any());
           
        }

        /// <summary>
        /// The complete questionnaire export view factory testable.
        /// </summary>
        protected class CompleteQuestionnaireExportViewFactoryTestable : CompleteQuestionnaireExportViewFactory
        {
            public CompleteQuestionnaireExportViewFactoryTestable(
                IDenormalizerStorage<CompleteQuestionnaireStoreDocument> documentSession,
                IDenormalizerStorage<QuestionnaireDocument> templateStore)
                : base(documentSession,templateStore)
            {
            }

            public HeaderCollection BuildHeaderTestable(IGroup template)
            {
                return base.BuildHeader(template, new List<Guid>(), new List<AutoQuestionWithTriggers>());
            }
            public HeaderCollection BuildHeaderTestable(IGroup template, List<Guid> subObject, List<AutoQuestionWithTriggers> autoQuestions)
            {
                return base.BuildHeader(template, subObject,autoQuestions);
            }
        }
    }
}
