using System;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.QuestionOptionsRepositoryTests
{
    [TestFixture(TestOf = typeof(QuestionOptionsRepository))]
    public class QuestionOptionsRepositoryTests
    {
        [Test]
        public void when_call_GetOptionsForQuestion_should_call_get_question_options_for_question_without_categorical_flag()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaire = CreateQuestionnaireWithOneCategoricalQuestion(questionId);
            var optionsRepository = Mock.Of<IOptionsRepository>();

            var storage = new QuestionOptionsRepository(optionsRepository);

            storage.GetOptionsForQuestion(questionnaire, questionId, null, null, null);

            Mock.Get(optionsRepository).Verify(s => s.GetFilteredQuestionOptions(questionnaire.QuestionnaireIdentity, questionId, null, null, null, null), Times.Once);
            Mock.Get(optionsRepository).Verify(s => s.GetFilteredCategoriesOptions(questionnaire.QuestionnaireIdentity, It.IsAny<Guid>(), null, null, null, null), Times.Never);
        }

        [Test]
        public void when_call_GetOptionsForQuestion_should_call_get_categorical_options_for_question_with_categorical_flag()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var categoryId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var questionnaire = CreateQuestionnaireWithOneCategoricalQuestion(questionId, categoryId);
            var optionsRepository = Mock.Of<IOptionsRepository>();

            var storage = new QuestionOptionsRepository(optionsRepository);

            storage.GetOptionsForQuestion(questionnaire, questionId, null, null, null);

            Mock.Get(optionsRepository).Verify(s => s.GetFilteredQuestionOptions(questionnaire.QuestionnaireIdentity, questionId, null, null, null, null), Times.Never);
            Mock.Get(optionsRepository).Verify(s => s.GetFilteredCategoriesOptions(questionnaire.QuestionnaireIdentity, categoryId, null, null, null, null), Times.Once);
        }

        [Test]
        public void when_call_GetOptionForQuestionByOptionText_should_get_question_options_for_question_without_categorical_flag()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaire = CreateQuestionnaireWithOneCategoricalQuestion(questionId);
            var optionsRepository = Mock.Of<IOptionsRepository>();

            var storage = new QuestionOptionsRepository(optionsRepository);

            storage.GetOptionForQuestionByOptionText(questionnaire, questionId, null, null, null);

            Mock.Get(optionsRepository).Verify(s => s.GetQuestionOption(questionnaire.QuestionnaireIdentity, questionId, null, null, null), Times.Once);
            Mock.Get(optionsRepository).Verify(s => s.GetCategoryOption(questionnaire.QuestionnaireIdentity, It.IsAny<Guid>(), null, null, null), Times.Never);
        }

        [Test]
        public void when_call_GetOptionForQuestionByOptionText_should_get_categorical_options_for_question_with_categorical_flag()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var categoryId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var questionnaire = CreateQuestionnaireWithOneCategoricalQuestion(questionId, categoryId);
            var optionsRepository = Mock.Of<IOptionsRepository>();

            var storage = new QuestionOptionsRepository(optionsRepository);

            storage.GetOptionForQuestionByOptionText(questionnaire, questionId, null, null, null);

            Mock.Get(optionsRepository).Verify(s => s.GetQuestionOption(questionnaire.QuestionnaireIdentity, questionId, null, null, null), Times.Never);
            Mock.Get(optionsRepository).Verify(s => s.GetCategoryOption(questionnaire.QuestionnaireIdentity, categoryId, null, null, null), Times.Once);
        }

        [Test]
        public void when_call_GetOptionForQuestionByOptionValue_should_get_question_options_for_question_without_categorical_flag()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaire = CreateQuestionnaireWithOneCategoricalQuestion(questionId);
            var optionsRepository = Mock.Of<IOptionsRepository>();

            var storage = new QuestionOptionsRepository(optionsRepository);

            storage.GetOptionForQuestionByOptionValue(questionnaire, questionId, 7, null, null);

            Mock.Get(optionsRepository).Verify(s => s.GetQuestionOptionByValue(questionnaire.QuestionnaireIdentity, questionId, 7, null, null), Times.Once);
            Mock.Get(optionsRepository).Verify(s => s.GetCategoryOptionByValue(questionnaire.QuestionnaireIdentity, It.IsAny<Guid>(), 7, null, null), Times.Never);
        }

        [Test]
        public void when_call_GetOptionForQuestionByOptionValue_should_get_categorical_options_for_question_with_categorical_flag()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var categoryId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var questionnaire = CreateQuestionnaireWithOneCategoricalQuestion(questionId, categoryId);
            var optionsRepository = Mock.Of<IOptionsRepository>();

            var storage = new QuestionOptionsRepository(optionsRepository);

            storage.GetOptionForQuestionByOptionValue(questionnaire, questionId, 7, null, null);

            Mock.Get(optionsRepository).Verify(s => s.GetQuestionOptionByValue(questionnaire.QuestionnaireIdentity, questionId, 7, null, null), Times.Never);
            Mock.Get(optionsRepository).Verify(s => s.GetCategoryOptionByValue(questionnaire.QuestionnaireIdentity, categoryId, 7, null, null), Times.Once);
        }

        [Test]
        public void when_call_GetOptionsByOptionValues_should_get_question_options_for_question_without_categorical_flag()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var optionValues = new int[] {1, 3, 7};
            var questionnaire = CreateQuestionnaireWithOneCategoricalQuestion(questionId);
            var optionsRepository = Mock.Of<IOptionsRepository>();

            var storage = new QuestionOptionsRepository(optionsRepository);

            storage.GetOptionsByOptionValues(questionnaire, questionId, optionValues, null);

            Mock.Get(optionsRepository).Verify(s => s.GetOptionsByValues(questionnaire.QuestionnaireIdentity, questionId, optionValues, null), Times.Once);
            Mock.Get(optionsRepository).Verify(s => s.GetCategoryOptionsByValues(questionnaire.QuestionnaireIdentity, It.IsAny<Guid>(), optionValues, null), Times.Never);
        }

        [Test]
        public void when_call_GetOptionsByOptionValues_should_get_categorical_options_for_question_with_categorical_flag()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var categoryId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var optionValues = new int[] {1, 3, 7};
            var questionnaire = CreateQuestionnaireWithOneCategoricalQuestion(questionId, categoryId);
            var optionsRepository = Mock.Of<IOptionsRepository>();

            var storage = new QuestionOptionsRepository(optionsRepository);

            storage.GetOptionsByOptionValues(questionnaire, questionId, optionValues, null);

            Mock.Get(optionsRepository).Verify(s => s.GetOptionsByValues(questionnaire.QuestionnaireIdentity, questionId, optionValues, null), Times.Never);
            Mock.Get(optionsRepository).Verify(s => s.GetCategoryOptionsByValues(questionnaire.QuestionnaireIdentity, categoryId, optionValues, null), Times.Once);
        }



        private static PlainQuestionnaire CreateQuestionnaireWithOneCategoricalQuestion(Guid questionId, Guid? reusableCategoryId = null)
        {
            SingleQuestion question = Create.Entity.SingleOptionQuestion(questionId, categoryId: reusableCategoryId);
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(question);
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            return questionnaire;
        }
    }
}
