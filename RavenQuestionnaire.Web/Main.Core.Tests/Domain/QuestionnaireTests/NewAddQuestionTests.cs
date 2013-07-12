using System;
using System.Linq;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using Ncqrs.Spec;

namespace Main.Core.Tests.Domain.QuestionnaireTests
{
    [TestFixture]
    public class NewAddQuestionTests : QuestionnaireARTestContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_AnswerTitleIsNotEmpty_Then_event_contains_the_same_answer_title(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                var questionnaireKey = Guid.NewGuid();
                var groupKey = Guid.NewGuid();
                // arrange
                QuestionnaireAR questionnsire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(questionnaireKey, groupKey);
                Option[] options = new Option[1] { new Option(Guid.NewGuid(), "1", "title") };

                // act
                questionnsire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, options, Order.AsIs, null, new Guid[0]);
                // assert
                var risedEvent = QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.AreEqual("title", risedEvent.Answers[0].AnswerText);
            }
        }


        [Test]
        public void NewAddQuestion_When_question_is_featured_but_inside_propagated_group_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid autoGroupId = Guid.NewGuid();
            bool isFeatured = true;
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupId);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(Guid.NewGuid(), autoGroupId, "What is your last name?", QuestionType.Text, "name", false,
                                                            isFeatured,
                                                            false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0, new Guid[0]);

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionIsFeaturedButNotInsideNonPropagateGroup));
        }

        [Test]
        public void NewAddQuestion_When_question_is_featured_and_inside_non_propagated_group_Then_raised_NewQuestionAdded_event_contains_the_same_featured_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid groupId = Guid.NewGuid();
                bool isFeatured = true;
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneNonPropagatedGroup(groupId: groupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?",
                                             QuestionType.Text, "name",
                                             false, isFeatured, false, QuestionScope.Interviewer, "", "", "", "",
                                             new Option[0], Order.AsIs, 0, new Guid[0]);

                // Assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext).Featured, Is.EqualTo(isFeatured));
            }
        }

        [Test]
        public void NewAddQuestion_When_Title_is_not_empty_Then_NewQuestionAdded_event_contains_the_same_title_caption()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var questionnaireKey = Guid.NewGuid();
                var groupKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(questionnaireKey, groupKey);

                var notEmptyTitle = "any not empty title";

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, notEmptyTitle, QuestionType.Text, "test", false,
                                             false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty,
                                             string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, new Guid[0]);

                // Assert
                var risedEvent = QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.That(risedEvent.QuestionText, Is.EqualTo(notEmptyTitle));
            }
        }

        [Test]
        public void NewAddQuestion_When_Title_is_empty_Then_DomainException_should_be_thrown()
        {
            // arrange
            var questionnaireKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(questionnaireKey, groupKey);

            // act
            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "", QuestionType.Text, "test", false, false,
                                                            false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                            string.Empty, new Option[0], Order.AZ, null, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionTitleRequired));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_AnswerTitleIsAbsent_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            var questionnaireKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            // arrange
            QuestionnaireAR questionnsire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(questionnaireKey, groupKey);

            Option[] options = new Option[1] { new Option(Guid.NewGuid(), "1", string.Empty) };
            // act
            TestDelegate act =
                () =>
                questionnsire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, options, Order.AsIs, null, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorTextRequired));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_AnswerTitleIsNotUnique_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            var groupKey = Guid.NewGuid();
            // arrange
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupKey);

            Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "title"), new Option(Guid.NewGuid(), "2", "title") };

            // act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, options, Order.AsIs, null, new Guid[0]);
            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorTextNotUnique));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_AnswerTitleIsUnique_Then_event_contains_the_same_answer_titles(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                var groupKey = Guid.NewGuid();
                // arrange
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupKey);

                Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "title1"), new Option(Guid.NewGuid(), "2", "title2") };
                // act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, options, Order.AsIs, null, new Guid[0]);
                // assert
                var risedEvent = QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext);
                for (int i = 0; i < options.Length; i++)
                {
                    Assert.IsTrue(options[i].Title == risedEvent.Answers[i].AnswerText);
                }
            }
        }

        [Test]
        public void NewAddQuestion_When_question_is_head_of_propagated_group_but_inside_non_propagated_group_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid groupId = Guid.NewGuid();
            bool isHeadOfPropagatedGroup = true;
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupId);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                                                            "name", false, false,
                                                            isHeadOfPropagatedGroup,
                                                            QuestionScope.Interviewer, "", "", "", "",
                                                            new Option[0], Order.AsIs, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionIsHeadOfGroupButNotInsidePropagateGroup));
        }

        [Test]
        public void NewAddQuestion_When_question_is_head_of_propagated_group_and_inside_propagated_group_Then_raised_NewQuestionAdded_event_contains_the_same_header_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid autoGroupId = Guid.NewGuid();
                bool isHeadOfPropagatedGroup = true;
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), autoGroupId, "What is your last name?",
                                             QuestionType.Text, "name",
                                             false, false, isHeadOfPropagatedGroup, QuestionScope.Interviewer, "", "", "", "",
                                             new Option[0], Order.AsIs, 0, new Guid[0]);

                // Assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext).Capital, Is.EqualTo(isHeadOfPropagatedGroup));
            }
        }

        [Test]
        public void NewAddQuestion_When_capital_parameter_is_true_Then_in_NewQuestionAdded_event_capital_property_should_be_set_in_true_too()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid groupId = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneAutoPropagatedGroup(groupId: groupId);
                bool capital = true;

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text, "name",
                                             false, false, capital, QuestionScope.Interviewer, "", "", "", "", new Option[] { }, Order.AZ, null, new Guid[] { });

                // Assert
                var risedEvent = QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.AreEqual(capital, risedEvent.Capital);
            }
        }

        [TestCase("ma_name38")]
        [TestCase("__")]
        [TestCase("_123456789012345678901234567890_")]
        public void NewAddQuestion_When_variable_name_is_valid_Then_rised_NewQuestionAdded_event_contains_the_same_stata_caption(string validVariableName)
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                                             validVariableName,
                                             false, false, false, QuestionScope.Interviewer,
                                             "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0]);


                // Assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext).StataExportCaption, Is.EqualTo(validVariableName));
            }
        }

        [Test]
        public void NewAddQuestion_When_variable_name_has_trailing_spaces_and_is_valid_Then_rised_NewQuestionAdded_event_should_contains_trimed_stata_caption()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupId);
                string longVariableName = " my_name38  ";

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                                             longVariableName,
                                             false, false, false, QuestionScope.Interviewer,
                                             "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0]);

                // Assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext).StataExportCaption, Is.EqualTo(longVariableName.Trim()));
            }
        }

        [Test]
        public void NewAddQuestion_When_variable_name_has_33_chars_Then_DomainException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();
            string longVariableName = "".PadRight(33, 'A');

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), Guid.NewGuid(), "What is your last name?", QuestionType.Text,
                                                                  longVariableName,
                                                                  false, false, false, QuestionScope.Interviewer,
                                                                  "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameMaxLength));
        }

        [Test]
        public void NewAddQuestion_When_variable_name_starts_with_digit_Then_DomainException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();

            string stataExportCaptionWithFirstDigit = "1aaaa";

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), Guid.NewGuid(), "What is your last name?", QuestionType.Text,
                                                                  stataExportCaptionWithFirstDigit,
                                                                  false, false, false, QuestionScope.Interviewer,
                                                                  "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameStartWithDigit));
        }

        [Test]
        public void NewAddQuestion_When_variable_name_is_empty_Then_DomainException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();

            string emptyVariableName = string.Empty;

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), Guid.NewGuid(), "What is your last name?", QuestionType.Text,
                                                                  emptyVariableName,
                                                                  false, false, false, QuestionScope.Interviewer,
                                                                  "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0]);


            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameRequired));
        }

        [Test]
        public void NewAddQuestion_When_variable_name_contains_any_non_underscore_letter_or_digit_character_Then_DomainException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();

            string nonValidVariableNameWithBannedSymbols = "aaa:_&b";

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), Guid.NewGuid(), "What is your last name?", QuestionType.Text,
                                                                  nonValidVariableNameWithBannedSymbols,
                                                                  false, false, false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameSpecialCharacters));
        }

        [Test]
        public void NewAddQuestion_When_questionnaire_has_another_question_with_same_variable_name_Then_DomainException_should_be_thrown()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var duplicateVariableName = "text";
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroupAndQuestionInIt(Guid.NewGuid(), groupId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                                                                  duplicateVariableName,
                                                                  false, false, false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0]);


            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VarialbeNameNotUnique));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_QuestionType_is_option_type_and_answer_options_list_is_empty_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // Arrange
            var emptyAnswersList = new Option[] { };
            Guid groupId = Guid.NewGuid();

            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupId);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?",
                                                            questionType, "name", false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                            emptyAnswersList, Order.AZ, null, new Guid[] { });

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorEmpty));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
#warning Roma: when part is incorrect should be something like when answer option value contains not number
        public void NewAddQuestion_When_answer_option_value_allows_only_numbers_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();

            // Act
            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(
                                   questionId: Guid.NewGuid(),
                                   groupId: Guid.NewGuid(),
                                   title: "What is your last name?",
                                   type: questionType,
                                   alias: "name",
                                   isMandatory: false,
                                   isFeatured: false,
                                   isHeaderOfPropagatableGroup: false,
                                   scope: QuestionScope.Interviewer,
                                   condition: string.Empty,
                                   validationExpression: string.Empty,
                                   validationMessage: string.Empty,
                                   instructions: string.Empty,
                                   optionsOrder: Order.AsIs,
                                   maxValue: 0,
                                   triggedGroupIds: new Guid[0],
                                   options: new Option[1] { new Option(id: Guid.NewGuid(), value: "some value", title: "text") });

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorValueSpecialCharacters));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_answer_option_value_contains_only_numbers_Then_raised_NewQuestionAdded_event_contains_question_answer_with_only_numbers_value(
            QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                string answerValue = "10";
                Guid autoGroupId = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupId);

                // act
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: autoGroupId,
                    title: "What is your last name?",
                    type: questionType,
                    alias: "name",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    optionsOrder: Order.AsIs,
                    maxValue: 0,
                    triggedGroupIds: new Guid[0],
                    options: new Option[1] { new Option(id: Guid.NewGuid(), value: answerValue, title: "text") });


                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext).Answers[0].AnswerValue, Is.EqualTo("10"));
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        [TestCase(QuestionType.Numeric)]
        [TestCase(QuestionType.Text)]
        [TestCase(QuestionType.DateTime)]
        [TestCase(QuestionType.AutoPropagate)]
        public void NewAddQuestion_When_question_type_is_allowed_Then_raised_NewQuestionAdded_event_with_same_question_type(
            QuestionType allowedQuestionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId);

                // act
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: groupId,
                    title: "What is your last name?",
                    type: allowedQuestionType,
                    alias: "name",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    optionsOrder: Order.AsIs,
                    maxValue: null,
                    triggedGroupIds: new Guid[]{},
                    options: QuestionnaireARTestContext.AreOptionsRequiredByQuestionType(allowedQuestionType) ? QuestionnaireARTestContext.CreateTwoOptions() : null);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext).QuestionType, Is.EqualTo(allowedQuestionType));
            }
        }

        [Test]
        [TestCase(QuestionType.DropDownList)]
        [TestCase(QuestionType.GpsCoordinates)]
        [TestCase(QuestionType.YesNo)]
        public void NewAddQuestion_When_question_type_is_not_allowed_Then_DomainException_with_type_NotAllowedQuestionType_should_be_thrown(
            QuestionType notAllowedQuestionType)
        {
            // arrange
            Guid groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId);

            // act
            TestDelegate act = () => questionnaire.NewAddQuestion(
                questionId: Guid.NewGuid(),
                groupId: groupId,
                title: "What is your last name?",
                type: notAllowedQuestionType,
                alias: "name",
                isMandatory: false,
                isFeatured: false,
                isHeaderOfPropagatableGroup: false,
                scope: QuestionScope.Interviewer,
                condition: string.Empty,
                validationExpression: string.Empty,
                validationMessage: string.Empty,
                instructions: string.Empty,
                optionsOrder: Order.AsIs,
                maxValue: null,
                triggedGroupIds: null,
                options: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.NotAllowedQuestionType));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_answer_option_value_is_required_Then_DomainException_should_be_thrown(QuestionType questionTypeWithOptionsExpected)
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(
                                   questionId: Guid.NewGuid(),
                                   groupId: groupId,
                                   title: "What is your last name?",
                                   type: questionTypeWithOptionsExpected,
                                   alias: "name",
                                   isMandatory: false,
                                   isFeatured: false,
                                   isHeaderOfPropagatableGroup: false,
                                   scope: QuestionScope.Interviewer,
                                   condition: string.Empty,
                                   validationExpression: string.Empty,
                                   validationMessage: string.Empty,
                                   instructions: string.Empty,
                                   optionsOrder: Order.AZ,
                                   maxValue: 0,
                                   triggedGroupIds: new Guid[] { },
                                   options: new Option[1] { new Option(id: Guid.NewGuid(), value: null, title: "text") });

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorValueRequired));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_answer_option_value_is_not_null_or_empty_Then_raised_NewQuestionAdded_event_contains_not_null_and_not_empty_question_answer(
            QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                string answerValue = "10";
                Guid autoGroupId = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupId);

                // act
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: autoGroupId,
                    title: "What is your last name?",
                    type: questionType,
                    alias: "name",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    optionsOrder: Order.AsIs,
                    maxValue: 0,
                    triggedGroupIds: new Guid[0],
                    options: new Option[1] { new Option(id: Guid.NewGuid(), value: answerValue, title: "text") });


                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext).Answers[0].AnswerValue, Is.EqualTo("10"));
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_answer_option_values_not_unique_in_options_scope_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            var questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();

            // Act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: Guid.NewGuid(),
                    title: "What is your last name?",
                    type: questionType,
                    alias: "name",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    optionsOrder: Order.AsIs,
                    maxValue: 0,
                    triggedGroupIds: new Guid[0],
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
        public void NewAddQuestion_When_answer_option_values_unique_in_options_scope_Then_raised_NewQuestionAdded_event_contains_only_unique_values_in_answer_values_scope(
            QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid autoGroupId = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupId);

                // act
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: autoGroupId,
                    title: "What is your last name?",
                    type: questionType,
                    alias: "name",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    optionsOrder: Order.AsIs,
                    maxValue: 0,
                    triggedGroupIds: new Guid[0],
                    options:
                        new Option[2]
                            {
                                new Option(id: Guid.NewGuid(), title: "text 1", value: "1"),
                                new Option(id: Guid.NewGuid(), title: "text 2", value: "2")
                            });


                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext).Answers.Select(x => x.AnswerValue).Distinct().Count(),
                            Is.EqualTo(2));
            }
        }

        [Test]
        public void NewAddQuestion_When_question_is_AutoPropagate_and_list_of_triggers_is_null_Then_rised_NewQuestionAdded_event_should_contains_null_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                var autoPropagate = QuestionType.AutoPropagate;
                Guid[] emptyTriggedGroupIds = null;
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, emptyTriggedGroupIds);


                // Assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext).Triggers, Is.Null);
            }
        }

        [Test]
        public void NewAddQuestion_When_question_is_AutoPropagate_and_list_of_triggers_is_empty_Then_rised_NewQuestionAdded_event_should_contains_empty_list_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                var autoPropagate = QuestionType.AutoPropagate;
                var emptyTriggedGroupIds = new Guid[0];
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, emptyTriggedGroupIds);


                // Assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext).Triggers, Is.Empty);
            }
        }

        [Test]
        public void NewAddQuestion_When_question_is_AutoPropagate_and_list_of_triggers_contains_absent_group_id_Then_DomainException_of_type_TriggerLinksToNotExistingGroup_should_be_thrown()
        {
            // Arrange
            var autoPropagate = QuestionType.AutoPropagate;

            var groupId = Guid.NewGuid();
            var absentGroupId = Guid.NewGuid();
            var triggedGroupIdsWithAbsentGroupId = new[] { absentGroupId };

            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                                                  false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                                  string.Empty, new Option[0], Order.AZ, null, triggedGroupIdsWithAbsentGroupId);


            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TriggerLinksToNotExistingGroup));
        }

        [Test]
        public void NewAddQuestion_When_question_is_AutoPropagate_and_list_of_triggers_contains_non_propagate_group_id_Then_DomainException_of_type_TriggerLinksToNotPropagatedGroup_should_be_thrown()
        {
            // Arrange
            var autoPropagate = QuestionType.AutoPropagate;
            var nonPropagateGroupId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var triggedGroupIdsWithNonPropagateGroupId = new[] { nonPropagateGroupId };

            QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithTwoGroups(nonPropagateGroupId, groupId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                                                  false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                                  string.Empty, new Option[0], Order.AZ, null, triggedGroupIdsWithNonPropagateGroupId);


            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TriggerLinksToNotPropagatedGroup));
        }

        [Test]
        public void NewAddQuestion_When_question_is_AutoPropagate_and_list_of_triggers_contains_propagate_group_id_Then_rised_NewQuestionAdded_event_should_contains_that_group_id_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var autoPropagate = QuestionType.AutoPropagate;
                var autoPropagateGroupId = Guid.NewGuid();
                var groupId = Guid.NewGuid();
                var triggedGroupIdsWithAutoPropagateGroupId = new[] { autoPropagateGroupId };

                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithAutoGroupAndRegularGroup(autoPropagateGroupId, groupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false,
                                             false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty,
                                             string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null,
                                             triggedGroupIdsWithAutoPropagateGroupId);

                // Assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<NewQuestionAdded>(eventContext).Triggers, Contains.Item(autoPropagateGroupId));
            }
        }
    }
}