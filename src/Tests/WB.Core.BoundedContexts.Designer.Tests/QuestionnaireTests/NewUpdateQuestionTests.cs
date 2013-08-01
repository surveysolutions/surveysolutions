using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    [TestFixture]
    public class NewUpdateQuestionTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void NewUpdateQuestion_When_Title_is_empty_Then_QuestionChanged_event_contains_the_same_title_caption()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid questionKey = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneQuestion(questionKey);

                string notEmptyTitle = "not empty :)";

                // act
                questionnaire.NewUpdateQuestion(questionKey, notEmptyTitle, QuestionType.Text, "test", false, false,
                                                false, QuestionScope.Interviewer, string.Empty, string.Empty,
                                                string.Empty,
                                                string.Empty, new Option[0], Order.AZ, null, new Guid[0]);

                // assert
                var risedEvent = GetSingleEvent<QuestionChanged>(eventContext);
                Assert.AreEqual(notEmptyTitle, risedEvent.QuestionText);
            }
        }

        [Test]
        public void NewUpdateQuestion_When_Title_is_empty_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid questionKey = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneQuestion(questionKey);

            // act
            TestDelegate act = () =>
                               questionnaire.NewUpdateQuestion(questionKey, "", QuestionType.Text, "test", false, false,
                                                               false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                               string.Empty, new Option[0], Order.AZ, null, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionTitleRequired));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_AnswerTitle_is_absent_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            Guid questionKey = Guid.NewGuid();
            // arrange
            Questionnaire questionnaire = CreateQuestionnaireWithOneQuestionnInTypeAndOptions(questionKey, questionType, new[] { new Option(Guid.NewGuid(), "123", "title") , new Option(Guid.NewGuid(), "2", "title1")});
            Option[] options = new Option[2] { new Option(Guid.NewGuid(), "1", string.Empty), new Option(Guid.NewGuid(), "2", string.Empty) };
            // act
            TestDelegate act =
                () =>
                questionnaire.NewUpdateQuestion(questionKey, "test", questionType, "test", false, false, false,
                                                QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                string.Empty, options, Order.AsIs, null, new Guid[0]);
            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorTextRequired));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_AnswerTitle_is_not_empty_Then_event_contains_the_same_answer_title(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                Guid questionKey = Guid.NewGuid();
                var notEmptyAnswerOptionTitle1 = "title";
                var notEmptyAnswerOptionTitle2 = "title1";
                Option[] options = new Option[2] { new Option(Guid.NewGuid(), "1", notEmptyAnswerOptionTitle1), new Option(Guid.NewGuid(), "2", notEmptyAnswerOptionTitle2) };
                // arrange
                Questionnaire questionnaire = CreateQuestionnaireWithOneQuestionnInTypeAndOptions(
                    questionKey, questionType, new[]
                        {
                            new Option(Guid.NewGuid(), "1", "option text"),
                            new Option(Guid.NewGuid(), "2", "option text1"),
                        });


                // act
                questionnaire.NewUpdateQuestion(questionKey, "test", questionType, "test", false, false, false,
                                                QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                string.Empty, options, Order.AsIs, null, new Guid[0]);
                // assert
                var risedEvent = GetSingleEvent<QuestionChanged>(eventContext);
                Assert.AreEqual(notEmptyAnswerOptionTitle1, risedEvent.Answers[0].AnswerText);
                Assert.AreEqual(notEmptyAnswerOptionTitle2, risedEvent.Answers[1].AnswerText);
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_AnswerTitle_is_not_unique_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            Guid questionKey = Guid.NewGuid();
            // arrange
            Questionnaire questionnaire = CreateQuestionnaireWithOneQuestionnInTypeAndOptions(questionKey, questionType, options: new[] { new Option(Guid.NewGuid(), "12", "title"), new Option(Guid.NewGuid(), "125", "title1") });
            Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "title"), new Option(Guid.NewGuid(), "2", "title") };
            // act
            TestDelegate act =
                () =>
                questionnaire.NewUpdateQuestion(questionKey, "test", questionType, "test", false, false, false,
                                                QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                string.Empty, options, Order.AsIs, null, new Guid[0]);
            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorTextNotUnique));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_AnswerTitle_is_unique_Then_event_contains_the_same_answer_titles(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var firstAnswerOptionTitle = "title1";
                var secondAnswerOptionTitleThatNotEqualsFirstOne = firstAnswerOptionTitle + "1";

                Guid questionKey = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneQuestionnInTypeAndOptions(questionKey, questionType, options: new[] { new Option(Guid.NewGuid(), "121", "title"), new Option(Guid.NewGuid(), "12", "title1") });
                Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", firstAnswerOptionTitle), new Option(Guid.NewGuid(), "2", secondAnswerOptionTitleThatNotEqualsFirstOne) };
                
                // act
                questionnaire.NewUpdateQuestion(questionKey, "test", questionType, "test", false, false, false,
                                                QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                string.Empty, options, Order.AsIs, null, new Guid[0]);
                // assert
                var risedEvent = GetSingleEvent<QuestionChanged>(eventContext);

                Assert.That(risedEvent.Answers[0].AnswerText, Is.EqualTo(firstAnswerOptionTitle));
                Assert.That(risedEvent.Answers[1].AnswerText, Is.EqualTo(secondAnswerOptionTitleThatNotEqualsFirstOne));
                
            }
        }

        [Test]
        public void NewUpdateQuestion_When_qustion_in_propagated_group_is_featured_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid updatedQuestion = Guid.NewGuid();
            bool isFeatured = true;
            Questionnaire questionnaire = CreateQuestionnaireWithOneAutoGroupAndQuestionInIt(updatedQuestion);

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(updatedQuestion, "What is your last name?", QuestionType.Text, "name", false,
                                                                     isFeatured,
                                                                     false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionIsFeaturedButNotInsideNonPropagateGroup));
        }

        [Test]
        public void NewUpdateQuestion_When_question_inside_non_propagated_group_is_featured_Then_raised_QuestionChanged_event_contains_the_same_featured_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid updatedQuestion = Guid.NewGuid();
                bool isFeatured = true;
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroupAndQuestionInIt(updatedQuestion);

                // Act
                questionnaire.NewUpdateQuestion(updatedQuestion, "What is your last name?", QuestionType.Text, "name", false,
                                                isFeatured,
                                                false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0, new Guid[0]);

                // Assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Featured, Is.EqualTo(isFeatured));
            }
        }

        [Test]
        public void NewUpdateQuestion_When_question_is_head_of_propagated_group_but_inside_non_propagated_group_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid updatedQuestion = Guid.NewGuid();
            bool isHeadOfPropagatedGroup = true;
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroupAndQuestionInIt(updatedQuestion);

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(updatedQuestion, "What is your last name?", QuestionType.Text, "name", false, false,
                                                                     isHeadOfPropagatedGroup,
                                                                     QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionIsHeadOfGroupButNotInsidePropagateGroup));
        }

        [Test]
        public void NewUpdateQuestion_When_question_is_head_of_propagated_group_and_inside_propagated_group_Then_raised_QuestionChanged_event_contains_the_same_header_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid updatedQuestion = Guid.NewGuid();
                bool isHeadOfPropagatedGroup = true;
                Questionnaire questionnaire = CreateQuestionnaireWithOneAutoGroupAndQuestionInIt(updatedQuestion);

                // Act
                questionnaire.NewUpdateQuestion(updatedQuestion, "What is your last name?", QuestionType.Text, "name", false, false,
                                                isHeadOfPropagatedGroup,
                                                QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0, new Guid[0]);

                // Assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Capital, Is.EqualTo(isHeadOfPropagatedGroup));
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_QuestionType_is_option_type_and_answer_options_list_is_empty_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // Arrange
            var emptyAnswersList = new Option[] { };

            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", questionType, "name",
                                                               false, false, false, QuestionScope.Interviewer, "", "", "",
                                                               "", emptyAnswersList, Order.AZ, 0, new List<Guid>().ToArray());

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorEmpty));
        }

        [Test]
        public void NewUpdateQuestion_When_capital_parameter_is_true_Then_in_QuestionChanged_event_capital_property_should_be_set_in_true_too()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid targetQuestionPublicKey = Guid.NewGuid();
                var questionnaire = CreateQuestionnaireWithOneAutoGroupAndQuestionInIt(targetQuestionPublicKey);

                bool capital = true;

                // Act
                questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text, "title",
                                                false, false, capital, QuestionScope.Interviewer, "", "", "",
                                                "", new Option[] { }, Order.AZ, 0, new List<Guid>().ToArray());

                // Assert
                var risedEvent = GetSingleEvent<QuestionChanged>(eventContext);
                Assert.AreEqual(capital, risedEvent.Capital);
            }
        }

        [TestCase("ma_name38")]
        [TestCase("__")]
        [TestCase("_123456789012345678901234567890_")]
        public void NewUpdateQuestion_When_variable_name_is_valid_Then_rised_QuestionChanged_event_contains_the_same_stata_caption(string validVariableName)
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid targetQuestionPublicKey = Guid.NewGuid();
                var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);

                // Act
                questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text,
                                                validVariableName,
                                                false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                new Option[0], Order.AZ, 0, new Guid[0]);

                // Assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).StataExportCaption, Is.EqualTo(validVariableName));
            }
        }

        [Test]
        public void NewUpdateQuestion_When_we_updating_absent_question_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Questionnaire questionnaire = CreateQuestionnaire();

            // Act
            TestDelegate act = () =>
                               questionnaire.NewUpdateQuestion(Guid.NewGuid(), "Title", QuestionType.Text, "valid",
                                                               false, false, false, QuestionScope.Interviewer, "", "", "",
                                                               "", new Option[] { }, Order.AZ, 0, new List<Guid>().ToArray());

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionNotFound));
        }

        [Test]
        public void NewUpdateQuestion_When_variable_name_has_33_chars_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);
            string longVariableName = "".PadRight(33, 'A');

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text,
                                                                     longVariableName,
                                                                     false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                                     new Option[0], Order.AZ, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameMaxLength));
        }

        [Test]
        public void NewUpdateQuestion_When_variable_name_starts_with_digit_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);

            string stataExportCaptionWithFirstDigit = "1aaaa";

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text,
                                                                     stataExportCaptionWithFirstDigit,
                                                                     false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                                     new Option[0], Order.AZ, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameStartWithDigit));
        }

        [Test]
        public void NewUpdateQuestion_When_variable_name_has_trailing_spaces_and_is_valid_Then_rised_QuestionChanged_evend_should_contains_trimed_stata_caption()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid targetQuestionPublicKey = Guid.NewGuid();
                var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);
                string variableNameWithTrailingSpaces = " my_name38  ";

                // Act
                questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text,
                                                variableNameWithTrailingSpaces,
                                                false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                new Option[0], Order.AZ, 0, new Guid[0]);


                // Assert
                var risedEvent = GetSingleEvent<QuestionChanged>(eventContext);
                Assert.AreEqual(variableNameWithTrailingSpaces.Trim(), risedEvent.StataExportCaption);
            }
        }

        [Test]
        public void NewUpdateQuestion_When_variable_name_is_empty_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);

            string emptyVariableName = string.Empty;

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text,
                                                                     emptyVariableName,
                                                                     false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                                     new Option[0], Order.AZ, 0, new Guid[0]);


            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameRequired));
        }

        [Test]
        public void NewUpdateQuestion_When_variable_name_contains_any_non_underscore_letter_or_digit_character_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);

            string nonValidVariableNameWithBannedSymbols = "aaa:_&b";

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text,
                                                                     nonValidVariableNameWithBannedSymbols,
                                                                     false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                                     new Option[0], Order.AZ, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameSpecialCharacters));
        }

        [Test]
        public void NewUpdateQuestion_When_questionnaire_has_another_question_with_same_variable_name_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey = Guid.NewGuid();
            string duplicateVariableName = "text";
            var questionnaire = CreateQuestionnaireWithTwoQuestions(targetQuestionPublicKey);

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text,
                                                                     duplicateVariableName,
                                                                     false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                                     new Option[0], Order.AZ, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VarialbeNameNotUnique));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
#warning Roma: when part is incorrect should be something like when answer option value contains not number
        public void NewUpdateQuestion_When_answer_option_value_allows_only_numbers_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewUpdateQuestion(
                                   questionId: targetQuestionPublicKey,
                                   title: "What is your last name?",
                                   alias: "name",
                                   type: QuestionType.MultyOption,
                                   scope: QuestionScope.Interviewer,
                                   condition: string.Empty,
                                   validationExpression: string.Empty,
                                   validationMessage: string.Empty,
                                   isFeatured: false,
                                   isMandatory: false,
                                   isHeaderOfPropagatableGroup: false,
                                   optionsOrder: Order.AZ,
                                   instructions: string.Empty,
                                   triggedGroupIds: new Guid[0],
                                   maxValue: 0,
                                   options: new Option[1] { new Option(id: Guid.NewGuid(), title: "text", value: "some text") });

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorValueSpecialCharacters));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_answer_option_value_contains_only_numbers_Then_raised_QuestionChanged_event_contains_question_answer_with_the_same_answe_values(
            QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid targetQuestionPublicKey = Guid.NewGuid();
                string answerValue1 = "10";
                string answerValue2 = "100";
                var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);

                // act
                questionnaire.NewUpdateQuestion(
                    questionId: targetQuestionPublicKey,
                    title: "What is your last name?",
                    alias: "name",
                    type: questionType,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    isFeatured: false,
                    isMandatory: false,
                    isHeaderOfPropagatableGroup: false,
                    optionsOrder: Order.AZ,
                    instructions: string.Empty,
                    triggedGroupIds: new Guid[0],
                    maxValue: 0,
                    options: new Option[2] { 
                        new Option(id: Guid.NewGuid(), title: "text1", value: answerValue1),
                        new Option(id: Guid.NewGuid(), title: "text2", value: answerValue2)});


                // assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Answers[0].AnswerValue, Is.EqualTo(answerValue1));
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Answers[1].AnswerValue, Is.EqualTo(answerValue2));
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        [TestCase(QuestionType.Numeric)]
        [TestCase(QuestionType.Text)]
        [TestCase(QuestionType.DateTime)]
        [TestCase(QuestionType.AutoPropagate)]
        public void NewUpdateQuestion_When_question_type_is_allowed_Then_raised_QuestionChanged_event_with_same_question_type(
            QuestionType allowedQuestionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid questionId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneQuestion(questionId);

                // act
                questionnaire.NewUpdateQuestion(
                    questionId: questionId,
                    title: "What is your last name?",
                    alias: "name",
                    type: allowedQuestionType,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    isFeatured: false,
                    isMandatory: false,
                    isHeaderOfPropagatableGroup: false,
                    optionsOrder: Order.AZ,
                    instructions: string.Empty,
                    triggedGroupIds: new Guid[0],
                    maxValue: 0,
                    options: AreOptionsRequiredByQuestionType(allowedQuestionType) ? CreateTwoOptions() : null);

                // assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).QuestionType, Is.EqualTo(allowedQuestionType));
            }
        }

        [Test]
        [TestCase(QuestionType.DropDownList)]
        [TestCase(QuestionType.GpsCoordinates)]
        [TestCase(QuestionType.YesNo)]
        public void NewUpdateQuestion_When_question_type_is_not_allowed_Then_DomainException_with_type_NotAllowedQuestionType_should_be_thrown(
            QuestionType notAllowedQuestionType)
        {
            // arrange
            Guid questionId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneQuestion(questionId);

            // act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(
                questionId: questionId,
                title: "What is your last name?",
                alias: "name",
                type: notAllowedQuestionType,
                scope: QuestionScope.Interviewer,
                condition: string.Empty,
                validationExpression: string.Empty,
                validationMessage: string.Empty,
                isFeatured: false,
                isMandatory: false,
                isHeaderOfPropagatableGroup: false,
                optionsOrder: Order.AZ,
                instructions: string.Empty,
                triggedGroupIds: new Guid[0],
                maxValue: 0,
                options: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.NotAllowedQuestionType));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_answer_option_value_is_required_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewUpdateQuestion(
                                   questionId: targetQuestionPublicKey,
                                   title: "What is your last name?",
                                   alias: "name",
                                   type: questionType,
                                   scope: QuestionScope.Interviewer,
                                   condition: string.Empty,
                                   validationExpression: string.Empty,
                                   validationMessage: string.Empty,
                                   isFeatured: false,
                                   isMandatory: false,
                                   isHeaderOfPropagatableGroup: false,
                                   optionsOrder: Order.AZ,
                                   instructions: string.Empty,
                                   triggedGroupIds: new Guid[0],
                                   maxValue: 0,
                                   options: new Option[1] { new Option(id: Guid.NewGuid(), title: "text", value: null) });

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorValueRequired));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_answer_option_value_is_not_null_or_empty_Then_raised_QuestionChanged_event_contains_not_null_and_not_empty_question_answer(
            QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid targetQuestionPublicKey = Guid.NewGuid();
                string notEmptyAnswerValue1 = "10";
                string notEmptyAnswerValue2 = "100";
                var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);

                // act
                questionnaire.NewUpdateQuestion(
                    questionId: targetQuestionPublicKey,
                    title: "What is your last name?",
                    alias: "name",
                    type: questionType,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    isFeatured: false,
                    isMandatory: false,
                    isHeaderOfPropagatableGroup: false,
                    optionsOrder: Order.AZ,
                    instructions: string.Empty,
                    triggedGroupIds: new Guid[0],
                    maxValue: 0,
                    options: new Option[2]
                        {
                            new Option(id: Guid.NewGuid(), title: "text", value: notEmptyAnswerValue1),
                            new Option(id: Guid.NewGuid(), title: "text1", value: notEmptyAnswerValue2)
                        });


                // assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Answers[0].AnswerValue, !Is.Empty);
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Answers[1].AnswerValue, !Is.Empty);
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_answer_option_values_not_unique_in_options_scope_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewUpdateQuestion(
                                   questionId: targetQuestionPublicKey,
                                   title: "What is your last name?",
                                   alias: "name",
                                   type: questionType,
                                   scope: QuestionScope.Interviewer,
                                   condition: string.Empty,
                                   validationExpression: string.Empty,
                                   validationMessage: string.Empty,
                                   isFeatured: false,
                                   isMandatory: false,
                                   isHeaderOfPropagatableGroup: false,
                                   optionsOrder: Order.AZ,
                                   instructions: string.Empty,
                                   triggedGroupIds: new Guid[0],
                                   maxValue: 0,
                                   options:
                                       new Option[2]
                                           {
                                               new Option(id: Guid.NewGuid(), value: "1", title: "text 1"),
                                               new Option(id: Guid.NewGuid(), value: "1", title: "text 2")
                                           });

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorValueNotUnique));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_answer_option_values_unique_in_options_scope_Then_raised_QuestionChanged_event_contains_only_unique_values_in_answer_values_scope(            QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid targetQuestionPublicKey = Guid.NewGuid();
                var questionnaire = CreateQuestionnaireWithOneQuestion(targetQuestionPublicKey);

                // act
                questionnaire.NewUpdateQuestion(
                    questionId: targetQuestionPublicKey,
                    title: "What is your last name?",
                    alias: "name",
                    type: questionType,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    isFeatured: false,
                    isMandatory: false,
                    isHeaderOfPropagatableGroup: false,
                    optionsOrder: Order.AZ,
                    instructions: string.Empty,
                    triggedGroupIds: new Guid[0],
                    maxValue: 0,
                    options:
                        new Option[2]
                            {
                                new Option(id: Guid.NewGuid(), title: "text 1", value: "1"),
                                new Option(id: Guid.NewGuid(), title: "text 2", value: "2")
                            });


                // assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Answers.Select(x => x.AnswerValue).Distinct().Count(),
                            Is.EqualTo(2));
            }
        }

        [Test]
        public void NewUpdateQuestion_When_question_is_AutoPropagate_and_list_of_triggers_is_null_Then_rised_QuestionChanged_event_should_contains_null_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                var autoPropagateQuestionId = Guid.NewGuid();
                var autoPropagate = QuestionType.AutoPropagate;
                Guid[] emptyTriggedGroupIds = null;
                Questionnaire questionnaire = CreateQuestionnaireWithOneAutoGroupAndQuestionInIt(autoPropagateQuestionId);

                // Act
                questionnaire.NewUpdateQuestion(autoPropagateQuestionId, "What is your last name?", autoPropagate, "name", false, false,
                                                false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                                emptyTriggedGroupIds);


                // Assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Triggers, Is.Null);
            }
        }

        [Test]
        public void NewUpdateQuestion_When_question_is_AutoPropagate_and_list_of_triggers_is_empty_Then_rised_QuestionChanged_event_should_contains_empty_list_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                var autoPropagateQuestionId = Guid.NewGuid();
                var autoPropagate = QuestionType.AutoPropagate;
                var emptyTriggedGroupIds = new Guid[0];
                Questionnaire questionnaire = CreateQuestionnaireWithOneAutoGroupAndQuestionInIt(autoPropagateQuestionId);

                // Act
                questionnaire.NewUpdateQuestion(autoPropagateQuestionId, "What is your last name?", autoPropagate, "name", false, false,
                                                false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                                emptyTriggedGroupIds);


                // Assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Triggers, Is.Empty);
            }
        }

        [Test]
        public void NewUpdateQuestion_When_question_is_AutoPropagate_and_list_of_triggers_contains_absent_group_id_Then_DomainException_should_be_thrown()
        {
            // Arrange
            var autoPropagate = QuestionType.AutoPropagate;
            var autoPropagateQuestionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var absentGroupId = Guid.NewGuid();
            var triggedGroupIdsWithAbsentGroupId = new[] { absentGroupId };

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroupAndQuestionInIt(autoPropagateQuestionId, groupId, questionType: QuestionType.AutoPropagate);

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(autoPropagateQuestionId, "What is your last name?", autoPropagate, "name", false, false,
                                                                     false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                                                     triggedGroupIdsWithAbsentGroupId);

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TriggerLinksToNotExistingGroup));
        }

        [Test]
        public void NewUpdateQuestion_When_question_is_AutoPropagate_and_list_of_triggers_contains_non_propagate_group_id_Then_DomainException_should_be_thrown()
        {
            // Arrange
            var autoPropagate = QuestionType.AutoPropagate;
            var autoPropagateQuestionId = Guid.NewGuid();
            var nonPropagateGroupId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var triggedGroupIdsWithNonPropagateGroupId = new[] { nonPropagateGroupId };

            Questionnaire questionnaire = CreateQuestionnaireWithTwoRegularGroupsAndQuestionInLast(nonPropagateGroupId, autoPropagateQuestionId);

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(autoPropagateQuestionId, "What is your last name?", autoPropagate, "name", false, false,
                                                                     false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                                                     triggedGroupIdsWithNonPropagateGroupId);


            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TriggerLinksToNotPropagatedGroup));
        }

        [Test]
        public void NewUpdateQuestion_When_question_is_AutoPropagate_and_list_of_triggers_contains_propagate_group_id_Then_rised_QuestionChanged_event_should_contains_that_group_id_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var autoPropagate = QuestionType.AutoPropagate;
                var autoPropagateQuestionId = Guid.NewGuid();
                var autoPropagateGroupId = Guid.NewGuid();
                var groupId = Guid.NewGuid();
                var triggedGroupIdsWithAutoPropagateGroupId = new[] { autoPropagateGroupId };

                Questionnaire questionnaire = CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionInIt(autoPropagateGroupId, groupId, autoPropagateQuestionId);

                // Act
                questionnaire.NewUpdateQuestion(autoPropagateQuestionId, "What is your last name?", autoPropagate, "name", false, false,
                                                false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                                triggedGroupIdsWithAutoPropagateGroupId);

                // Assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Triggers, Contains.Item(autoPropagateGroupId));
            }
        }
    }
}