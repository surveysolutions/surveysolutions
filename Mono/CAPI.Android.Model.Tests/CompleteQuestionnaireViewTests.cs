using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.GridItems;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.Validation;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;

namespace CAPI.Androids.Core.Model.Tests
{
    [TestFixture]
    public class CompleteQuestionnaireViewTests
    {
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
                                                      false, false, "", "");
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
                                                      false, false, "", "");
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
                () => target.SetAnswer(new ItemPublicKey(Guid.NewGuid(), null),null,null));
        }
        [Test]
        public void SetAnswer_QuestionISPresent_AnswerSetAndValidationExecuted()
        {
            var questions = new Dictionary<ItemPublicKey, QuestionViewModel>();
            var questionKEy = new ItemPublicKey(Guid.NewGuid(), null);
            var question = new ValueQuestionViewModel(questionKEy, "test", QuestionType.Text, "t", true, "", "", false,
                                                      false, false, "", "");

            Mock<IQuestionnaireValidationExecutor> validator = new Mock<IQuestionnaireValidationExecutor>();
            questions.Add(questionKEy, question);
            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(questions, validator.Object);
            target.SetAnswer(questionKEy, null,"answer");
            Assert.AreEqual(question.AnswerString, "answer");
            validator.Verify(x => x.Execute(), Times.Once());
        }
        [Test]
        public void PropagateGroup_TemplateISAbsent_ExeptionThrown()
        {
            var templates = new TemplateCollection();
            var screens = new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(screens,templates);
            Assert.Throws<KeyNotFoundException>(
                () => target.PropagateGroup(Guid.NewGuid(), Guid.NewGuid()));
        }
        [Test]
        public void PropagateGroup_TemplateISPresent_QuestionAndScreenHashAreUpdated()
        {
            var templates = new TemplateCollection();
            var templateKey = new ItemPublicKey(Guid.NewGuid(), null);
            var questionKey = Guid.NewGuid();
            var template = new QuestionnairePropagatedScreenViewModel(Guid.NewGuid(), "t", true,
                                                            templateKey,
                                                            new IQuestionnaireItemViewModel[1]
                                                                {
                                                                    new ValueQuestionViewModel(
                                                                new ItemPublicKey(questionKey, null), "test",
                                                                QuestionType.Text, "t", true, "", "", false,
                                                                false, false, "", "")
                                                                }, null,
                                                            Enumerable.Empty<ItemPublicKey>());
            var grid = new QuestionnaireGridViewModel(template.QuestionnaireId, "t", "t", templateKey, true,
                                                      Enumerable.Empty<ItemPublicKey>(),
                                                      Enumerable.Empty<ItemPublicKey>(),
                                                    //  Enumerable.Empty<QuestionnaireScreenViewModel>(),
                                                      new List<HeaderItem>(),
                                                      Enumerable.Empty<QuestionnairePropagatedScreenViewModel>);
            var screens = new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
            screens.Add(templateKey, grid);
            templates.Add(templateKey.PublicKey, template);
            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(screens,templates);
            var propagationKey = Guid.NewGuid();
            target.PropagateGroup(templateKey.PublicKey, propagationKey);
            Assert.AreEqual(target.GetQuestionHash().Count, 1);
            var questionFromHash = target.GetQuestionHash().Select(q => q.Value).First();
            Assert.AreEqual(questionFromHash.PublicKey.PropagationKey, propagationKey);
            Assert.AreEqual(questionFromHash.PublicKey.PublicKey, questionKey);

            Assert.AreEqual(target.Screens.Count, 2);
            Assert.AreEqual(target.Screens[new ItemPublicKey(templateKey.PublicKey, propagationKey)].Title, "t");
        }
        [Test]
        public void RemovePropagatedGroup_GRoupISAbsent_ExeptionThrown()
        {
            var templates = new TemplateCollection();
            var screens = new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(screens, templates);
            Assert.Throws<KeyNotFoundException>(
                () => target.RemovePropagatedGroup(Guid.NewGuid(), Guid.NewGuid()));
        }
        [Test]
        public void RemovePropagatedGroup_GRoupISPresent_GroupDeleted()
        {
            //   var templates = new Dictionary<Guid, QuestionnaireScreenViewModel>();
            var screens = new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
            var questions = new Dictionary<ItemPublicKey, QuestionViewModel>();
            var propagationKey = Guid.NewGuid();
            var question = new ValueQuestionViewModel(
                new ItemPublicKey(Guid.NewGuid(), propagationKey), "test",
                QuestionType.Text, "t", true, "", "", false,
                false, false, "", "");
            questions.Add(question.PublicKey, question);
            var templateKey = new ItemPublicKey(Guid.NewGuid(), null);


            var propagatedGroup = new QuestionnairePropagatedScreenViewModel(Guid.NewGuid(), "t", true,
                                                                             new ItemPublicKey(templateKey.PublicKey,
                                                                                               propagationKey),
                                                                             new IQuestionnaireItemViewModel[1]
                                                                                 {
                                                                                     question
                                                                                 },
                                                                             (k) => Enumerable.Empty<ItemPublicKey>(),
                                                                             Enumerable.Empty<ItemPublicKey>());
            screens.Add(propagatedGroup.ScreenId, propagatedGroup);
            var grid = new QuestionnaireGridViewModel(propagatedGroup.QuestionnaireId, "t", "t", templateKey, true,
                                                      Enumerable.Empty<ItemPublicKey>(),
                                                      Enumerable.Empty<ItemPublicKey>(),
                                                      //Enumerable.Empty<QuestionnaireScreenViewModel>(),
                                                      new List<HeaderItem>(),
                                                      Enumerable.Empty<QuestionnairePropagatedScreenViewModel>);
            screens.Add(templateKey, grid);
            CompleteQuestionnaireViewTestable target = new CompleteQuestionnaireViewTestable(screens, questions);
            target.RemovePropagatedGroup(templateKey.PublicKey, propagationKey);
            Assert.AreEqual(target.GetQuestionHash().Count, 0);
            Assert.AreEqual(target.Screens.Count, 1);
        }
    }

    public class CompleteQuestionnaireViewTestable : CompleteQuestionnaireView
    {
        public CompleteQuestionnaireViewTestable(IDictionary<ItemPublicKey, QuestionViewModel> questions):this(Guid.NewGuid().ToString())
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
        public CompleteQuestionnaireViewTestable(IDictionary<ItemPublicKey, QuestionViewModel> questions, IQuestionnaireValidationExecutor mockValidator):this(questions)
        {
            this.validator = mockValidator;
        }
        public CompleteQuestionnaireViewTestable(IDictionary<ItemPublicKey, IQuestionnaireViewModel> screents)
            : this(Guid.NewGuid().ToString())
        {
            this.Screens = screents;
        }
        public CompleteQuestionnaireViewTestable(string publicKey) : base(publicKey)
        {
            this.Questions=new Dictionary<ItemPublicKey, QuestionViewModel>();
            this.Templates = new TemplateCollection();
            this.Screens=new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
        }

        public CompleteQuestionnaireViewTestable(CompleteQuestionnaireDocument document) : base(document)
        {
        }
        public IDictionary<ItemPublicKey, QuestionViewModel> GetQuestionHash()
        {
            return this.Questions;
        }
    }

}
