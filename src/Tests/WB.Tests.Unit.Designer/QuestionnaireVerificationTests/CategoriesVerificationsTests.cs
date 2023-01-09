using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestOf(typeof(CategoriesVerifications))]
    internal class CategoriesVerificationsTests : QuestionnaireVerifierTestsContext
    {
        [TestCase("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "WB0289")]
        [TestCase("variable_", "WB0290")]
        [TestCase("vari__able", "WB0291")]
        public void when_name_is_invalid(string name, string errorCode)
            => Create.QuestionnaireDocument("v", categories: new[] {Create.Categories(name: name)}).ExpectError(errorCode);

        [TestCase(" ", "WB0292")]
        [TestCase("abstract", "WB0293")]
        [TestCase("variableЙФЪ", "WB0294")]
        [TestCase("_variable", "WB0295")]
        [TestCase("1variable", "WB0295")]
        public void when_name_is_critically_invalid(string name, string errorCode)
            => Create.QuestionnaireDocument("v", categories: new[] {Create.Categories(name: name)}).ExpectCritical(errorCode);

        [Test]
        public void when_verifying_questionnaire_with_reusable_categories_and_parent_and_ids_are_not_unique()
        {
            // assert
            var questionId = Guid.Parse("10000000000000000000000000000000");
            var parentQuestionId = Guid.Parse("22222222222222222222222222222222");
            var categoriesId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = CreateQuestionnaireDocument(
                Create.SingleOptionQuestion
                (
                    parentQuestionId,
                    variable: "parentQuestion",
                    answers: new List<Answer>
                    {
                        new Answer {AnswerText = "opt 1", AnswerValue = "1"},
                        new Answer {AnswerText = "opt 2", AnswerValue = "2"},
                    }
                ),
                Create.SingleOptionQuestion
                (
                    questionId,
                    variable: "question",
                    categoriesId: categoriesId,
                    cascadeFromQuestionId: parentQuestionId
                ));

            questionnaire.Categories.Add(new Categories
            {
                Id = categoriesId,
                Name = "name"
            });

            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == new List<CategoriesItem>()
                {
                    new CategoriesItem {Id = 1, ParentId = 2, Text = "child 2"},
                    new CategoriesItem {Id = 1, ParentId = 2, Text = "child 3"}
                }.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire));

            // arrange
            verificationMessages.ShouldContainError("WB0305");
            verificationMessages.Single(e => e.Code == "WB0305").MessageLevel.Should().Be(VerificationMessageLevel.General);
            verificationMessages.Single(e => e.Code == "WB0305").References.Count().Should().Be(1);
            verificationMessages.Single(e => e.Code == "WB0305").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Categories);
            verificationMessages.Single(e => e.Code == "WB0305").References.First().Id.Should().Be(categoriesId);
        }
        
        [Test]
        public void when_verifying_questionnaire_with_reusable_categories_and_parent_and_text_are_not_unique()
        {
            // assert
            var questionId = Guid.Parse("10000000000000000000000000000000");
            var parentQuestionId = Guid.Parse("22222222222222222222222222222222");
            var categoriesId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = CreateQuestionnaireDocument(
                Create.SingleOptionQuestion
                (
                    parentQuestionId,
                    variable: "parentQuestion",
                    answers: new List<Answer>
                    {
                        new Answer {AnswerText = "opt 1", AnswerValue = "1"},
                        new Answer {AnswerText = "opt 2", AnswerValue = "2"},
                    }
                ),
                Create.SingleOptionQuestion
                (
                    questionId,
                    variable: "question",
                    categoriesId: categoriesId,
                    cascadeFromQuestionId: parentQuestionId
                ));

            questionnaire.Categories.Add(new Categories
            {
                Id = categoriesId,
                Name = "name"
            });

            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == new List<CategoriesItem>()
                {
                    new CategoriesItem {Id = 1, ParentId = 2, Text = "child 2"},
                    new CategoriesItem {Id = 3, ParentId = 2, Text = "child 2"}
                }.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire));

            // arrange
            verificationMessages.ShouldContainError("WB0306");
            verificationMessages.Single(e => e.Code == "WB0306").MessageLevel.Should().Be(VerificationMessageLevel.General);
            verificationMessages.Single(e => e.Code == "WB0306").References.Count().Should().Be(1);
            verificationMessages.Single(e => e.Code == "WB0306").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Categories);
            verificationMessages.Single(e => e.Code == "WB0306").References.First().Id.Should().Be(categoriesId);
        }
        
        [Test]
        public void when_verifying_questionnaire_with_reusable_categories_and_category_doesnt_have_options()
        {
            // assert
            var questionId = Guid.Parse("10000000000000000000000000000000");
            var categoriesId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = CreateQuestionnaireDocument(
                Create.SingleOptionQuestion
                (
                    questionId,
                    variable: "question",
                    categoriesId: categoriesId
                ));

            questionnaire.Categories.Add(new Categories
            {
                Id = categoriesId,
                Name = "name"
            });

            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == new List<CategoriesItem>().AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire));

            // arrange
            verificationMessages.ShouldContainCritical("WB0312");
            var verificationMessage = verificationMessages.Single(e => e.Code == "WB0312");
            verificationMessage.MessageLevel.Should().Be(VerificationMessageLevel.Critical);
            verificationMessage.References.Count().Should().Be(1);
            verificationMessage.References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Categories);
            verificationMessage.References.First().Id.Should().Be(categoriesId);
        }

    }
}
