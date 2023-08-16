using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using NHibernate.Collection.Generic;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestOf(typeof(QuestionVerifications))]
    internal class QuestionVerificationsTests : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void when_categorical_multi_question_has_more_than_allowed_options_should_return_WB0075()
        {
            // arrange
            Guid filteredComboboxId = Guid.Parse("10000000000000000000000000000000");
            int incrementer = 0;
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(
                    filteredComboboxId,
                    variable: "var",
                    filteredCombobox: true,
                    options:
                    new List<Answer>(
                        new Answer[15001].Select(
                            answer =>
                                new Answer()
                                {
                                    AnswerValue = incrementer.ToString(),
                                    AnswerText = (incrementer++).ToString()
                                }))
                ));

            QuestionnaireVerifier verifier = CreateQuestionnaireVerifier();

            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire));

            // assert
            verificationMessages.Count().Should().Be(1);

            verificationMessages.Single().Code.Should().Be("WB0075");

            verificationMessages.Single().References.Count().Should().Be(1);

            verificationMessages.Single().References.First().Type.Should()
                .Be(QuestionnaireVerificationReferenceType.Question);

            verificationMessages.Single().References.First().Id.Should().Be(filteredComboboxId);
        }

        [Test]
        public void when_verifying_categorical_multi_and_options_count_more_than_200()
        {
            Guid multiOptionId = Guid.Parse("10000000000000000000000000000000");
            int incrementer = 0;
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(
                    multiOptionId,
                    options:
                    new List<Answer>(
                        new Answer[201].Select(
                            answer =>
                                new Answer()
                                {
                                    AnswerValue = incrementer.ToString(),
                                    AnswerText = (incrementer++).ToString()
                                }))
                )
            );

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire));

            verificationMessages.ShouldContainError("WB0076");

            verificationMessages.Single(e => e.Code == "WB0076").MessageLevel.Should()
                .Be(VerificationMessageLevel.General);

            verificationMessages.Single(e => e.Code == "WB0076").References.Count().Should().Be(1);

            verificationMessages.Single(e => e.Code == "WB0076").References.First().Type.Should()
                .Be(QuestionnaireVerificationReferenceType.Question);

            verificationMessages.Single(e => e.Code == "WB0076").References.First().Id.Should().Be(multiOptionId);
        }

        [Test]
        public void when_verifying_questionnaire_with_categorical_multi_answers_question_that_has_max_allowed_answers_count_more_than_reusable_categories_count()
        {
            // arrange
            Guid multyOptionsQuestionId = Guid.Parse("10000000000000000000000000000000");
            Guid categoriesId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = CreateQuestionnaireDocument(Create.MultyOptionsQuestion(
                multyOptionsQuestionId,
                categoriesId: categoriesId,
                maxAllowedAnswers: 3,
                variable: "var1"
            ));
            
            questionnaire.Categories = new List<Categories>()
            {
                new Categories(){ Id = categoriesId, Name = "test"}
            };
            
            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == new List<CategoriesItem>()
                {
                    new CategoriesItem(){ Id = 1, Text = "opt1"},
                    new CategoriesItem{Id = 2, Text = "opt2"}
                }.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);
            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire)).ToList();

            // assert
            verificationMessages.Count().Should().Be(1);
            verificationMessages.Single().Code.Should().Be("WB0021");
            verificationMessages.Single().MessageLevel.Should().Be(VerificationMessageLevel.General);
        }

        [Test]
        public void when_verifying_questionnaire_has_question_with_incorrect_referrence_to_reusable_category()
        {
            // arrange
            Guid multyOptionsQuestionId = Guid.Parse("10000000000000000000000000000000");
            Guid categoriesId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = CreateQuestionnaireDocument(Create.MultyOptionsQuestion(
                multyOptionsQuestionId,
                categoriesId: categoriesId,
                variable: "var1"
            ));
            
            var verifier = CreateQuestionnaireVerifier();
            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire)).ToList();

            // assert
            verificationMessages.Count().Should().Be(1);
            verificationMessages.Single().Code.Should().Be("WB0307");
            verificationMessages.Single().MessageLevel.Should().Be(VerificationMessageLevel.General);
        }

        [Test]
        public void when_categorical_single_question_with_reusable_categories_has_option_values_that_doesnt_exit_in_parent_question()
        {
            // arrange
            var parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            var childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
            var childCategoriesId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion {
                    PublicKey = parentSingleOptionQuestionId,
                    StataExportCaption = "var",
                    Answers = new List<Answer> {
                        new Answer { AnswerText = "one", AnswerValue = "1" },
                        new Answer { AnswerText = "two", AnswerValue = "2" }
                    }
                },
                new SingleQuestion
                {
                    PublicKey = childCascadedComboboxId,
                    StataExportCaption = "var1",
                    CascadeFromQuestionId = parentSingleOptionQuestionId,
                    CategoriesId = childCategoriesId
                }
            );
            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), childCategoriesId) == new List<CategoriesItem>()
                {
                    new CategoriesItem{ Id = 1, ParentId = 3, Text =  "child 1"},
                    new CategoriesItem{ Id = 2, ParentId = 4, Text =  "child 2"}
                }.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationErrors = Enumerable.ToList(verifier.GetAllErrors(Create.QuestionnaireView(questionnaire)));

            // assert
            verificationErrors.ShouldContainError("WB0084");
            verificationErrors.GetError("WB0084").References.Should().Contain(@ref => @ref.ItemId == parentSingleOptionQuestionId.FormatGuid());
            verificationErrors.GetError("WB0084").References.Should().Contain(@ref => @ref.ItemId == childCascadedComboboxId.FormatGuid());
            verificationErrors.GetError("WB0084").References.Should().OnlyContain(x => x.Type == QuestionnaireVerificationReferenceType.Question);
        }

        [Test]
        public void when_categorical_single_question_has_option_values_that_doesnt_exit_in_parent_question_with_reusable_categories()
        {
            // arrange
            var parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            var childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
            var parentCategoriesId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion {
                    PublicKey = parentSingleOptionQuestionId,
                    StataExportCaption = "var",
                    CategoriesId = parentCategoriesId
                },
                new SingleQuestion
                {
                    PublicKey = childCascadedComboboxId,
                    StataExportCaption = "var1",
                    CascadeFromQuestionId = parentSingleOptionQuestionId,
                    Answers = new List<Answer> {
                        new Answer { AnswerText = "child 1", AnswerValue = "1", ParentValue = "3" },
                        new Answer { AnswerText = "child 2", AnswerValue = "2", ParentValue = "4" },
                    }
                }
            );
            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), parentCategoriesId) == new List<CategoriesItem>()
                {
                    new CategoriesItem{ Id = 1,  Text =  "one"},
                    new CategoriesItem{ Id = 2, Text =  "two"}
                }.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationErrors = Enumerable.ToList(verifier.GetAllErrors(Create.QuestionnaireView(questionnaire)));

            // assert
            verificationErrors.ShouldContainError("WB0084");
            verificationErrors.GetError("WB0084").References.Should().Contain(@ref => @ref.ItemId == parentSingleOptionQuestionId.FormatGuid());
            verificationErrors.GetError("WB0084").References.Should().Contain(@ref => @ref.ItemId == childCascadedComboboxId.FormatGuid());
            verificationErrors.GetError("WB0084").References.Should().OnlyContain(x => x.Type == QuestionnaireVerificationReferenceType.Question);
        }

        [Test]
        public void when_categorical_single_question_with_reusable_categories_has_option_values_that_doesnt_exit_in_parent_question_with_reusable_categories()
        {
            // arrange
            var parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            var childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
            var childCategoriesId = Guid.Parse("11111111111111111111111111111111");
            var parentCategoriesId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion {
                    PublicKey = parentSingleOptionQuestionId,
                    StataExportCaption = "var",
                    CategoriesId = parentCategoriesId
                },
                new SingleQuestion
                {
                    PublicKey = childCascadedComboboxId,
                    StataExportCaption = "var1",
                    CascadeFromQuestionId = parentSingleOptionQuestionId,
                    CategoriesId = childCategoriesId
                }
            );
            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), parentCategoriesId) == new List<CategoriesItem>()
                {
                    new CategoriesItem{ Id = 1,  Text =  "one"},
                    new CategoriesItem{ Id = 2, Text =  "two"}
                }.AsQueryable() &&
                x.GetCategoriesById(It.IsAny<Guid>(), childCategoriesId) == new List<CategoriesItem>()
                {
                    new CategoriesItem{ Id = 1, ParentId = 3, Text =  "child 1"},
                    new CategoriesItem{ Id = 2, ParentId = 4, Text =  "child 2"}
                }.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationErrors = Enumerable.ToList(verifier.GetAllErrors(Create.QuestionnaireView(questionnaire)));

            // assert
            verificationErrors.ShouldContainError("WB0084");
            verificationErrors.GetError("WB0084").References.Should().Contain(@ref => @ref.ItemId == parentSingleOptionQuestionId.FormatGuid());
            verificationErrors.GetError("WB0084").References.Should().Contain(@ref => @ref.ItemId == childCascadedComboboxId.FormatGuid());
            verificationErrors.GetError("WB0084").References.Should().OnlyContain(x => x.Type == QuestionnaireVerificationReferenceType.Question);
        }

        [Test]
        public void when_verifying_categorical_multi_with_reusable_categories_and_options_count_more_than_200()
        {
            // arrange
            Guid multiOptionId = Guid.Parse("10000000000000000000000000000000");
            Guid categoriesId =  Guid.Parse("11111111111111111111111111111111");
            int incrementer = 0;

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(
                    multiOptionId,
                    categoriesId: categoriesId
                )
            );

            var autoGeneratedCategories = new CategoriesItem[201].Select(
                answer =>
                    new CategoriesItem()
                    {
                        Id = incrementer,
                        Text = (incrementer++).ToString()
                    });

            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == autoGeneratedCategories.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire));

            // arrange
            verificationMessages.ShouldContainError("WB0076");
            verificationMessages.Single(e => e.Code == "WB0076").MessageLevel.Should().Be(VerificationMessageLevel.General);
            verificationMessages.Single(e => e.Code == "WB0076").References.Count().Should().Be(1);
            verificationMessages.Single(e => e.Code == "WB0076").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
            verificationMessages.Single(e => e.Code == "WB0076").References.First().Id.Should().Be(multiOptionId);
        }

        [Test]
        public void when_verifying_categorical_single_with_reusable_categories_and_options_count_more_than_200()
        {
            // arrange
            Guid singleOptionId = Guid.Parse("10000000000000000000000000000000");
            Guid categoriesId =  Guid.Parse("11111111111111111111111111111111");
            int incrementer = 0;

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.SingleOptionQuestion(
                    singleOptionId,
                    categoriesId: categoriesId
                )
            );

            var autoGeneratedCategories = new CategoriesItem[201].Select(
                answer =>
                    new CategoriesItem()
                    {
                        Id = incrementer,
                        Text = (incrementer++).ToString()
                    });

            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == autoGeneratedCategories.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire));

            // arrange
            verificationMessages.ShouldContainError("WB0076");
            verificationMessages.Single(e => e.Code == "WB0076").MessageLevel.Should().Be(VerificationMessageLevel.General);
            verificationMessages.Single(e => e.Code == "WB0076").References.Count().Should().Be(1);
            verificationMessages.Single(e => e.Code == "WB0076").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
            verificationMessages.Single(e => e.Code == "WB0076").References.First().Id.Should().Be(singleOptionId);
        }

        [Test]
        public void when_verifying_questionnaire_with_multi_question_with_reusable_categories_and_ids_are_not_unique()
        {
            // assert
            var multiQuestionId = Guid.Parse("10000000000000000000000000000000");
            var categoriesId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = CreateQuestionnaireDocument(
                Create.MultyOptionsQuestion
                (
                    multiQuestionId,
                    variable: "var",
                    categoriesId: categoriesId
                ));

            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == new List<CategoriesItem>()
                {
                    new CategoriesItem{ Id = 1,  Text =  "one"},
                    new CategoriesItem{ Id = 1, Text =  "two"}
                }.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire));

            // arrange
            verificationMessages.ShouldContainError("WB0073");
            verificationMessages.Single(e => e.Code == "WB0073").MessageLevel.Should().Be(VerificationMessageLevel.General);
            verificationMessages.Single(e => e.Code == "WB0073").References.Count().Should().Be(1);
            verificationMessages.Single(e => e.Code == "WB0073").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
            verificationMessages.Single(e => e.Code == "WB0073").References.First().Id.Should().Be(multiQuestionId);
        }

        [Test]
        public void when_verifying_questionnaire_with_cascading_question_with_reusable_categories_and_ids_are_not_unique()
        {
            // assert
            var questionId = Guid.Parse("10000000000000000000000000000000");
            var categoriesId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = CreateQuestionnaireDocument(
                Create.SingleOptionQuestion
                (
                    questionId,
                    variable: "var",
                    categoriesId: categoriesId,
                    cascadeFromQuestionId: Guid.NewGuid()
                ));

            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == new List<CategoriesItem>()
                {
                    new CategoriesItem{ Id = 1,  Text =  "one"},
                    new CategoriesItem{ Id = 1, Text =  "two"}
                }.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire));

            // arrange
            verificationMessages.ShouldContainError("WB0073");
            verificationMessages.Single(e => e.Code == "WB0073").MessageLevel.Should().Be(VerificationMessageLevel.General);
            verificationMessages.Single(e => e.Code == "WB0073").References.Count().Should().Be(1);
            verificationMessages.Single(e => e.Code == "WB0073").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
            verificationMessages.Single(e => e.Code == "WB0073").References.First().Id.Should().Be(questionId);
        }

        [Test]
        public void when_verifying_questionnaire_with_single_question_with_reusable_categories_and_ids_are_not_unique()
        {
            // assert
            var questionId = Guid.Parse("10000000000000000000000000000000");
            var categoriesId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = CreateQuestionnaireDocument(
                Create.SingleOptionQuestion
                (
                    questionId,
                    variable: "var",
                    categoriesId: categoriesId
                ));

            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == new List<CategoriesItem>()
                {
                    new CategoriesItem{ Id = 1,  Text =  "one"},
                    new CategoriesItem{ Id = 1, Text =  "two"}
                }.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire));

            // arrange
            verificationMessages.ShouldContainError("WB0073");
            verificationMessages.Single(e => e.Code == "WB0073").MessageLevel.Should().Be(VerificationMessageLevel.General);
            verificationMessages.Single(e => e.Code == "WB0073").References.Count().Should().Be(1);
            verificationMessages.Single(e => e.Code == "WB0073").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
            verificationMessages.Single(e => e.Code == "WB0073").References.First().Id.Should().Be(questionId);
        }

        [Test]
        public void when_verifying_questionnaire_with_cascading_question_and_panent_with_reusable_categories_and_ids_are_not_unique()
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
                    categoriesId: categoriesId
                ),
                Create.SingleOptionQuestion
                (
                    questionId,
                    variable: "question",
                    cascadeFromQuestionId: parentQuestionId,
                    answers: new List<Answer>
                    {
                        new Answer {AnswerText = "opt 1", ParentValue = "1", AnswerValue = "1"},
                        new Answer {AnswerText = "opt 2", ParentValue = "1", AnswerValue = "1"},
                    }
                ));

            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoriesId) == new List<CategoriesItem>()
                {
                    new CategoriesItem {Id = 1, Text = "parent 1"},
                    new CategoriesItem {Id = 2, Text = "parent 2"}
                }.AsQueryable());

            var verifier = CreateQuestionnaireVerifier(reusableCategoriesService: categoriesService);

            // act
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire));

            // arrange
            verificationMessages.ShouldContainError("WB0073");
            verificationMessages.Single(e => e.Code == "WB0073").MessageLevel.Should().Be(VerificationMessageLevel.General);
            verificationMessages.Single(e => e.Code == "WB0073").References.Count().Should().Be(1);
            verificationMessages.Single(e => e.Code == "WB0073").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
            verificationMessages.Single(e => e.Code == "WB0073").References.First().Id.Should().Be(questionId);
        }

        [Test]
        public void when_verifying_questionnaire_with_question_inside_matrix_roster_with_substitutions_references_with_same_roster_level()
        {
            Guid questionWithSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            Guid sameRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
            string sameRosterLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
            var rosterGroupId1 =       Guid.Parse("AAAAAAAAAAAAAAAA1111111111111111");
            var rosterGroupId2 =       Guid.Parse("AAAAAAAAAAAAAAAA2222222222222222");
            var rosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.NumericIntegerQuestion(rosterSizeQuestionId, variable: "var1"),
                new Group()
                {
                    PublicKey = rosterGroupId1,
                    IsRoster = true,
                    DisplayMode = RosterDisplayMode.Matrix,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>()
                    {
                        Create.SingleOptionQuestion(
                            questionWithSubstitutionsId,
                            variable: "var2",
                            title: $"hello %{sameRosterLevelQuestionVariableName}%",
                            answers: new List<Answer> { new Answer(){ AnswerValue = "1", AnswerText = "opt 1" }, new Answer(){ AnswerValue = "2", AnswerText = "opt 2" }}
                        ),
                    }.ToReadOnlyCollection()

                },
                new Group()
                {
                    PublicKey = rosterGroupId2,
                    IsRoster = true,
                    VariableName = "c",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>()
                    {
                        Create.NumericRealQuestion(
                            sameRosterLevelQuestionId,
                            variable: sameRosterLevelQuestionVariableName
                        )
                    }.ToReadOnlyCollection()

                }
            });

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.GetAllErrors(Create.QuestionnaireView(questionnaire)).ToList();
            verificationMessages.ShouldContainError("WB0302");
            verificationMessages.GetError("WB0302").References.Count().Should().Be(2);
            verificationMessages.GetError("WB0302").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
            verificationMessages.GetError("WB0302").References.First().Id.Should().Be(questionWithSubstitutionsId);
            verificationMessages.GetError("WB0302").References.Last().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
            verificationMessages.GetError("WB0302").References.Last().Id.Should().Be(sameRosterLevelQuestionId);
        }
        
        [Test]
        public void when_question_on_cover_page_without_variable_label()
            => QuestionnaireDocumentWithCoverPage(new[]
                {
                    Create.Question(),
                })
                .ExpectError("WB0309");

        [TestCase(QuestionType.Audio)]
        [TestCase(QuestionType.Area)]
        [TestCase(QuestionType.QRBarcode)]
        [TestCase(QuestionType.TextList)]
        [TestCase(QuestionType.Multimedia)]
        [TestCase(QuestionType.MultyOption)]
        public void when_question_on_cover_page_allow_only_allowed_types(QuestionType questionType)
            => QuestionnaireDocumentWithCoverPage(new[]
                {
                    Create.Question(questionType: questionType),
                })
                .ExpectError("WB0308");
        
        [Test]
        public void when_question_in_matrix_roster()
            => QuestionnaireDocumentWithCoverPage(new[]
                {
                    Create.Roster(displayMode: RosterDisplayMode.Matrix, children: new[]
                    {
                        Create.Question()
                    }),
                })
                .ExpectNoWarning("WB0203");

    }
}
