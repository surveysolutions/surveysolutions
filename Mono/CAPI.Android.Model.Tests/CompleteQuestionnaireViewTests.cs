using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.GridItems;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace CAPI.Androids.Core.Model.Tests
{
    [TestFixture]
    public class CompleteQuestionnaireViewTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void SetScreenStatus_ScreenISAbsent_ExeptionThrown()
        {
            var screens = new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();

            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(screens);
            Assert.Throws<KeyNotFoundException>(
                () => target.SetScreenStatus(new ItemPublicKey(Guid.NewGuid(), null), true));
        }
        [Test]
        public void SetScreenStatus_ScreenISPresent_StatusIsChanged()
        {
            var screens = new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
            var screenKEy = new ItemPublicKey(Guid.NewGuid(), null);
            var screen = new QuestionnaireScreenViewModel(Guid.NewGuid(), "test", "test", true, screenKEy,
                                                          Enumerable.Empty<IQuestionnaireItemViewModel>().ToList(),
                                                          Enumerable.Empty<ItemPublicKey>(),
                                                          Enumerable.Empty<ItemPublicKey>());
            screens.Add(screenKEy, screen);
            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(screens);
            target.SetScreenStatus(screenKEy, false);
            Assert.AreEqual(screen.Enabled, false);
        }

        [Test]
        public void SetQuestionStatus_QuestionISAbsent_ExeptionISNotThrown()
        {
            var questions = new Dictionary<ItemPublicKey, QuestionViewModel>();

            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(questions);
            target.SetQuestionStatus(new ItemPublicKey(Guid.NewGuid(), null), true);
          /*  Assert.Throws<KeyNotFoundException>(
                () => target.SetQuestionStatus(new ItemPublicKey(Guid.NewGuid(), null), true));*/
        }
        [Test]
        public void SetQuestionStatus_QuestionISPresent_StatusIsChanged()
        {
            var questions = new Dictionary<ItemPublicKey, QuestionViewModel>();
            var questionKEy = new ItemPublicKey(Guid.NewGuid(), null);
            var question = new ValueQuestionViewModel(questionKEy, "test", QuestionType.Text, "t", true, "", "", false,
                                                      false, false,  "");
            questions.Add(questionKEy, question);
            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(questions);
            target.SetQuestionStatus(questionKEy, false);
            Assert.AreEqual(question.Status.HasFlag(QuestionStatus.Enabled), false);
        }
        [Test]
        public void SetComment_QuestionISAbsent_ExeptionThrown()
        {
            var questions = new Dictionary<ItemPublicKey, QuestionViewModel>();

            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(questions);
            Assert.Throws<KeyNotFoundException>(
                () => target.SetComment(new ItemPublicKey(Guid.NewGuid(), null), ""));
        }
        [Test]
        public void SetComment_QuestionISPresent_StatusIsChanged()
        {
            var questions = new Dictionary<ItemPublicKey, QuestionViewModel>();
            var questionKEy = new ItemPublicKey(Guid.NewGuid(), null);
            var question = new ValueQuestionViewModel(questionKEy, "test", QuestionType.Text, "t", true, "", "", false,
                                                      false, false,  "");
            questions.Add(questionKEy, question);
            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(questions);
            target.SetComment(questionKEy, "comment");
            Assert.AreEqual(question.Comments, "comment");
        }
        [Test]
        public void SetAnswer_QuestionISAbsent_ExeptionThrown()
        {
            var questions = new Dictionary<ItemPublicKey, QuestionViewModel>();

            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(questions);
            Assert.Throws<KeyNotFoundException>(
                () => target.SetAnswer(new ItemPublicKey(Guid.NewGuid(), null),null));
        }
        [Test]
        public void SetAnswer_QuestionISPresent_AnswerSetAndValidationExecuted()
        {
            var questions = new Dictionary<ItemPublicKey, QuestionViewModel>();
            var questionKEy = new ItemPublicKey(Guid.NewGuid(), null);
            var question = new ValueQuestionViewModel(questionKEy, "test", QuestionType.Text, "t", true, "", "", false,
                                                      false, false,  "");

            questions.Add(questionKEy, question);
            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(questions);
            target.SetAnswer(questionKEy, "answer");
            Assert.AreEqual(question.AnswerString, "answer");
        }
    }

    public class CompleteQuestionnaireViewTestable : CompleteQuestionnaireView
    {
        public CompleteQuestionnaireViewTestable(IDictionary<ItemPublicKey, QuestionViewModel> questions):this(Guid.NewGuid())
        {
            this.Questions = questions;
        }
        public CompleteQuestionnaireViewTestable(IDictionary<ItemPublicKey, IQuestionnaireViewModel> screents, TemplateCollection templates)
            : this(screents)
        {
            this.Templates = templates;
        }
        public CompleteQuestionnaireViewTestable(IDictionary<ItemPublicKey, IQuestionnaireViewModel> screents, IDictionary<ItemPublicKey, QuestionViewModel> questions)
            : this(screents)
        {
            this.Questions = questions;
        }
        public CompleteQuestionnaireViewTestable(IDictionary<ItemPublicKey, IQuestionnaireViewModel> screents)
            : this(Guid.NewGuid())
        {
            this.Screens = screents;
        }
        public CompleteQuestionnaireViewTestable(Guid publicKey) : base(publicKey)
        {
            this.Questions=new Dictionary<ItemPublicKey, QuestionViewModel>();
            this.Templates = new TemplateCollection();
            this.Screens=new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
        }

        public IDictionary<ItemPublicKey, QuestionViewModel> GetQuestionHash()
        {
            return this.Questions;
        }
    }

}
