using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Main.Core.Tests.Utils;
using NUnit.Framework;
using Ncqrs.Eventing;
using Ncqrs.Spec;
using System.Linq;

namespace Main.Core.Tests.Domain
{
    using Moq;

    using Ncqrs;

    [TestFixture]
    public class QuestionnaireARTests
    {
        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void QuestionnaireARConstructor_When_questionnaire_title_is_empty_or_contains_whitespaces_only_Then_throws_DomainException_with_type_QuestionnaireTitleRequired(string emptyTitle)
        {
            // arrange

            // act
            TestDelegate act = () => new QuestionnaireAR(Guid.NewGuid(), emptyTitle);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionnaireTitleRequired));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void UpdateQuestionnaire_When_questionnaire_title_is_empty_or_contains_whitespaces_only_Then_throws_DomainException_with_type_QuestionnaireTitleRequired(string emptyTitle)
        {
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            // act
            TestDelegate act = () => questionnaire.UpdateQuestionnaire(emptyTitle);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionnaireTitleRequired));
        }

        [Test]
        public void UpdateQuestionnaire_When_questionnaire_title_is_not_empty_Then_raised_QuestionnaireUpdated_event_contains_questionnaire_title()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var nonEmptyTitle = "Title";
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();

                // act
                questionnaire.UpdateQuestionnaire(nonEmptyTitle);

                // assert
                Assert.That(GetSingleEvent<QuestionnaireUpdated>(eventContext).Title, Is.EqualTo(nonEmptyTitle));
            }
        }

        [Test]
        public void NewAddQuestion_when_question_is_featured_but_inside_propagated_group_then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid autoGroupId = Guid.NewGuid();
            bool isFeatured = true;
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupId);

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
        public void NewAddQuestion_when_question_is_featured_and_inside_non_propagated_group_then_raised_NewQuestionAdded_event_contains_the_same_featured_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid groupId = Guid.NewGuid();
                bool isFeatured = true;
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneNonPropagatedGroup(groupId: groupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?",
                                                 QuestionType.Text, "name",
                                                 false, isFeatured, false, QuestionScope.Interviewer, "", "", "", "",
                                                 new Option[0], Order.AsIs, 0, new Guid[0]);

                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Featured, Is.EqualTo(isFeatured));
            }
        }

        #region empty title tests

        [Test]
        public void NewAddQuestion_When_Title_is_not_empty_Then_NewQuestionAdded_event_contains_the_same_title_caption()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var questionnaireKey = Guid.NewGuid();
                var groupKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(questionnaireKey, groupKey);

                var notEmptyTitle = "any not empty title";

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, notEmptyTitle, QuestionType.Text, "test", false,
                                             false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty,
                                             string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, new Guid[0]);

                // Assert
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.That(risedEvent.QuestionText, Is.EqualTo(notEmptyTitle));
            }
        }

        [Test]
        public void NewUpdateQuestion_When_Title_is_empty_Then_QuestionChanged_event_contains_the_same_title_caption()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid questionKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestion(questionKey);

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
        public void NewAddQuestion_When_Title_is_empty_Then_DomainException_should_be_thrown()
        {
            // arrange
            var questionnaireKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(questionnaireKey, groupKey);

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
        public void NewUpdateQuestion_When_Title_is_empty_Then_DomainException_should_be_thrown()
        {
            // arrange
            Guid questionKey = Guid.NewGuid();
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestion(questionKey);

            // act
            TestDelegate act = () =>
                questionnaire.NewUpdateQuestion(questionKey, "", QuestionType.Text, "test", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionTitleRequired));
        }
        #endregion

        #region answer option title is required

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_AnswerTitleIsAbsent_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            var questionnaireKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            // arrange
            QuestionnaireAR questionnsire = CreateQuestionnaireARWithOneGroup(questionnaireKey, groupKey);

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
        public void NewAddQuestion_When_AnswerTitleIsNotEmpty_Then_event_contains_the_same_answer_title(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                var questionnaireKey = Guid.NewGuid();
                var groupKey = Guid.NewGuid();
                // arrange
                QuestionnaireAR questionnsire = CreateQuestionnaireARWithOneGroup(questionnaireKey, groupKey);
                Option[] options = new Option[1] { new Option(Guid.NewGuid(), "1", "title") };

                // act
                questionnsire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, options, Order.AsIs, null, new Guid[0]);
                // assert
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.AreEqual("title", risedEvent.Answers[0].AnswerText);
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_AnswerTitleIsAbsent_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            Guid questionKey = Guid.NewGuid();
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestionnInTypeAndOptions(
                questionKey, questionType, new[] { new Option(Guid.NewGuid(), "123", "title") });
            Option[] options = new Option[1] { new Option(Guid.NewGuid(), "1", string.Empty) };
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
        public void NewUpdateQuestion_When_AnswerTitleIsNotEmpty_Then_event_contains_the_same_answer_title(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                Guid questionKey = Guid.NewGuid();
                Option[] options = new Option[1] { new Option(Guid.NewGuid(), "1", "title") };
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestionnInTypeAndOptions(
                    questionKey, questionType, new[]
                    {
                        new Option(Guid.NewGuid(), "1", "option text"),
                    });


                // act
                questionnaire.NewUpdateQuestion(questionKey, "test", questionType, "test", false, false, false,
                                                QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                string.Empty, options, Order.AsIs, null, new Guid[0]);
                // assert
                var risedEvent = GetSingleEvent<QuestionChanged>(eventContext);
                Assert.AreEqual("title", risedEvent.Answers[0].AnswerText);
            }
        }

        #endregion

        #region answer title is unique in question scope

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_AnswerTitleIsNotUnique_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            var groupKey = Guid.NewGuid();
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupKey);

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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupKey);

                Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "title1"), new Option(Guid.NewGuid(), "2", "title2") };
                // act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                         false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                         string.Empty, options, Order.AsIs, null, new Guid[0]);
                // assert
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                for (int i = 0; i < options.Length; i++)
                {
                    Assert.IsTrue(options[i].Title == risedEvent.Answers[i].AnswerText);
                }
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_AnswerTitleIsNotUnique_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            Guid questionKey = Guid.NewGuid();
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestionnInTypeAndOptions(questionKey, questionType, options: new[] { new Option(Guid.NewGuid(), "12", "title") });
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
        public void NewUpdateQuestion_When_AnswerTitleIsUnique_Then_event_contains_the_same_answer_titles(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                Guid questionKey = Guid.NewGuid();
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestionnInTypeAndOptions(questionKey, questionType, options: new[] { new Option(Guid.NewGuid(), "12", "title") });
                Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "title1"), new Option(Guid.NewGuid(), "2", "title2") };
                // act
                questionnaire.NewUpdateQuestion(questionKey, "test", questionType, "test", false, false, false,
                                                QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                string.Empty, options, Order.AsIs, null, new Guid[0]);
                // assert
                var risedEvent = GetSingleEvent<QuestionChanged>(eventContext);
                for (int i = 0; i < options.Length; i++)
                {
                    Assert.IsTrue(options[i].Title == risedEvent.Answers[i].AnswerText);
                }
            }
        }

        #endregion


        [Test]
        public void NewAddQuestion_when_question_is_head_of_propagated_group_but_inside_non_propagated_group_then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid groupId = Guid.NewGuid();
            bool isHeadOfPropagatedGroup = true;
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupId);

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
        public void NewAddQuestion_when_question_is_head_of_propagated_group_and_inside_propagated_group_then_raised_NewQuestionAdded_event_contains_the_same_header_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid autoGroupId = Guid.NewGuid();
                bool isHeadOfPropagatedGroup = true;
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), autoGroupId, "What is your last name?",
                                                 QuestionType.Text, "name",
                                                 false, false, isHeadOfPropagatedGroup, QuestionScope.Interviewer, "", "", "", "",
                                                 new Option[0], Order.AsIs, 0, new Guid[0]);

                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Capital, Is.EqualTo(isHeadOfPropagatedGroup));
            }
        }

        [Test]
        public void AddQuestion_When_capital_parameter_is_true_Then_in_NewQuestionAdded_event_capital_property_should_be_set_in_true_too()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid groupId = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoPropagatedGroup(groupId: groupId);
                bool capital = true;

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text, "name",
                    false, false, capital, QuestionScope.Interviewer, "", "", "", "", new Option[] { }, Order.AZ, null, new Guid[] { });

                // Assert
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                                             validVariableName,
                                             false, false, false, QuestionScope.Interviewer,
                                             "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0]);


                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).StataExportCaption, Is.EqualTo(validVariableName));
            }
        }

        [Test]
        public void NewAddQuestion_When_variable_name_has_trailing_spaces_and_is_valid_Then_rised_NewQuestionAdded_event_should_contains_trimed_stata_caption()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupId);
                string longVariableName = " my_name38  ";

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                                             longVariableName,
                                             false, false, false, QuestionScope.Interviewer,
                                             "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0]);

                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).StataExportCaption, Is.EqualTo(longVariableName.Trim()));
            }
        }

        [Test]
        public void NewAddQuestion_When_variable_name_has_33_chars_Then_DomainException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
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
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

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
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

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
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

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
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroupAndQuestionInIt(Guid.NewGuid(), groupId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                duplicateVariableName,
                false, false, false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0]);


            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VarialbeNameNotUnique));
        }

        [Test]
        public void NewUpdateQuestion_when_qustion_in_propagated_group_is_featured_then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid updatedQuestion = Guid.NewGuid();
            bool isFeatured = true;
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoGroupAndQuestionInIt(updatedQuestion);

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(updatedQuestion, "What is your last name?", QuestionType.Text, "name", false,
                                               isFeatured,
                                               false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionIsFeaturedButNotInsideNonPropagateGroup));
        }

        [Test]
        public void NewUpdateQuestion_when_question_inside_non_propagated_group_is_featured_then_raised_QuestionChanged_event_contains_the_same_featured_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid updatedQuestion = Guid.NewGuid();
                bool isFeatured = true;
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroupAndQuestionInIt(updatedQuestion);

                // Act
                questionnaire.NewUpdateQuestion(updatedQuestion, "What is your last name?", QuestionType.Text, "name", false,
                                               isFeatured,
                                               false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0, new Guid[0]);

                // Assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Featured, Is.EqualTo(isFeatured));
            }
        }

        [Test]
        public void NewUpdateQuestion_when_question_is_head_of_propagated_group_but_inside_non_propagated_group_then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid updatedQuestion = Guid.NewGuid();
            bool isHeadOfPropagatedGroup = true;
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroupAndQuestionInIt(updatedQuestion);

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(updatedQuestion, "What is your last name?", QuestionType.Text, "name", false, false,
                                                isHeadOfPropagatedGroup,
                                                QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0, new Guid[0]);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionIsHeadOfGroupButNotInsidePropagateGroup));
        }

        [Test]
        public void NewUpdateQuestion_when_question_is_head_of_propagated_group_and_inside_propagated_group_then_raised_QuestionChanged_event_contains_the_same_header_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid updatedQuestion = Guid.NewGuid();
                bool isHeadOfPropagatedGroup = true;
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoGroupAndQuestionInIt(updatedQuestion);

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
        public void AddQuestion_When_QuestionType_is_option_type_and_answer_options_list_is_empty_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // Arrange
            var emptyAnswersList = new Option[] { };
            Guid groupId = Guid.NewGuid();

            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupId);

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
        public void ChangeQuestion_When_QuestionType_is_option_type_and_answer_options_list_is_empty_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // Arrange
            var emptyAnswersList = new Option[] { };

            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);

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
        public void ChangeQuestion_When_capital_parameter_is_true_Then_in_QuestionChanged_event_capital_property_should_be_set_in_true_too()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid targetQuestionPublicKey = Guid.NewGuid();
                var questionnaire = CreateQuestionnaireARWithOneAutoGroupAndQuestionInIt(targetQuestionPublicKey);

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
                var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);

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
        public void ChangeQuestion_When_we_updating_absent_question_Then_DomainException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

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
            var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);
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
            var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);

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
                var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);
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
            var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);

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
            var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);

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
            var questionnaire = CreateQuestionnaireARWithTwoQuestions(targetQuestionPublicKey);

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
        public void DeleteGroup_When_group_public_key_specified_Then_raised_GroupDeleted_event_with_same_group_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);

                // act
                Guid parentPublicKey = Guid.NewGuid();
                questionnaire.NewDeleteGroup(groupPublicKey);

                // assert
                Assert.That(GetSingleEvent<GroupDeleted>(eventContext).GroupPublicKey, Is.EqualTo(groupPublicKey));
            }
        }

        [Test]
        public void DeleteImage_When_specified_keys_of_existing_question_and_image_Then_raised_ImageDeleted_event_with_specified_question_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var imageKey = Guid.NewGuid();
                var questionKey = Guid.NewGuid();
                var questionnaire = CreateQuestionnaireARWithOneQuestionAndOneImage(questionKey, imageKey);

                // act
                questionnaire.DeleteImage(questionKey, imageKey);

                // assert
                Assert.That(GetSingleEvent<ImageDeleted>(eventContext).QuestionKey, Is.EqualTo(questionKey));
            }
        }

        [Test]
        public void DeleteImage_When_specified_keys_of_existing_question_and_image_Then_raised_ImageDeleted_event_with_specified_image_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var imageKey = Guid.NewGuid();
                var questionKey = Guid.NewGuid();
                var questionnaire = CreateQuestionnaireARWithOneQuestionAndOneImage(questionKey, imageKey);

                // act
                questionnaire.DeleteImage(questionKey, imageKey);

                // assert
                Assert.That(GetSingleEvent<ImageDeleted>(eventContext).ImageKey, Is.EqualTo(imageKey));
            }
        }

        [Test]
        public void DeleteQuestion_When_question_id_specified_Then_raised_QuestionDeleted_event_with_same_question_id()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid questionId = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestion(questionId);

                // act
                Guid parentPublicKey = Guid.NewGuid();
                questionnaire.NewDeleteQuestion(questionId);

                // assert
                Assert.That(GetSingleEvent<QuestionDeleted>(eventContext).QuestionId, Is.EqualTo(questionId));
            }
        }

        [Test]
        public void MoveQuestionnaireItem_When_public_key_specified_Then_raised_QuestionnaireItemMoved_event_with_same_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                var publicKey = Guid.NewGuid();

                // act
                questionnaire.MoveQuestionnaireItem(publicKey, null, null);

                // assert
                Assert.That(GetSingleEvent<QuestionnaireItemMoved>(eventContext).PublicKey, Is.EqualTo(publicKey));
            }
        }

        [Test]
        public void MoveQuestionnaireItem_When_group_public_key_specified_Then_raised_QuestionnaireItemMoved_event_with_same_group_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                var groupPublicKey = Guid.NewGuid();

                // act
                questionnaire.MoveQuestionnaireItem(Guid.NewGuid(), groupPublicKey, null);

                // assert
                Assert.That(GetSingleEvent<QuestionnaireItemMoved>(eventContext).GroupKey, Is.EqualTo(groupPublicKey));
            }
        }

        [Test]
        public void MoveQuestionnaireItem_When_public_key_of_item_to_put_after_specified_Then_raised_QuestionnaireItemMoved_event_with_same_public_key_of_item_to_put_after()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                var afterItemPublicKey = Guid.NewGuid();

                // act
                questionnaire.MoveQuestionnaireItem(Guid.NewGuid(), null, afterItemPublicKey);

                // assert
                Assert.That(GetSingleEvent<QuestionnaireItemMoved>(eventContext).AfterItemKey, Is.EqualTo(afterItemPublicKey));
            }
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void NewUpdateGroup_When_groups_new_title_is_empty_or_whitespaces_Then_throws_DomainException(string emptyTitle)
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupPublicKey);

            // act
            TestDelegate act = () => questionnaire.NewUpdateGroup(groupPublicKey, emptyTitle, Propagate.None, null, null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupTitleRequired));
        }

        [Test]
        public void NewUpdateGroup_When_groups_new_title_is_not_empty_Then_raised_GroupUpdated_event_contains_the_same_group_title()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupPublicKey);
                string notEmptyNewTitle = "Some new title";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, notEmptyNewTitle, Propagate.None, null, null);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).GroupText, Is.EqualTo(notEmptyNewTitle));
            }
        }

        [Test]
        public void NewUpdateGroup_When_groups_propagation_kind_is_unsupported_Then_throws_DomainException()
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupPublicKey);
            var unsupportedPropagationKing = Propagate.Propagated;

            // act
            TestDelegate act = () => questionnaire.NewUpdateGroup(groupPublicKey, "Title", unsupportedPropagationKing, null, null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.NotSupportedPropagationGroup));
        }

        [TestCase(Propagate.None)]
        [TestCase(Propagate.AutoPropagated)]
        public void NewUpdateGroup_When_groups_propagation_kind_is_supported_Then_raised_GroupUpdated_event_contains_the_same_propagation_kind(Propagate supportedPopagationKind)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupPublicKey);

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "Title", supportedPopagationKind, null, null);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).Propagateble, Is.EqualTo(supportedPopagationKind));
            }
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void NewAddGroup_When_groups_title_is_empty_or_whitespaces_Then_throws_DomainException(string emptyTitle)
        {
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), null, emptyTitle, Propagate.None, null, null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupTitleRequired));
        }

        [Test]
        public void NewAddGroup_When_groups_title_is_not_empty_Then_raised_NewAddGroup_event_contains_the_same_group_title()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                string notEmptyNewTitle = "Some new title";

                // act
                questionnaire.NewAddGroup(Guid.NewGuid(), null, notEmptyNewTitle, Propagate.None, null, null);

                // assert
                Assert.That(GetSingleEvent<NewGroupAdded>(eventContext).GroupText, Is.EqualTo(notEmptyNewTitle));
            }
        }

        [Test]
        public void NewAddGroup_When_parent_group_has_AutoPropagate_propagation_kind_Then_throws_DomainException()
        {
            // arrange
            var parentAutoPropagateGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            questionnaire.AddChapter().AddGroup(parentAutoPropagateGroupId, propagationKind: Propagate.AutoPropagated);

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), parentAutoPropagateGroupId, "Title", Propagate.None, null, null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.AutoPropagateGroupCantHaveChildGroups));
        }

        [Test]
        public void NewAddGroup_When_parent_group_is_non_propagated_Then_raised_NewAddGroup_event_contains_regular_group_id_as_parent()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var parentRegularGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                questionnaire.AddChapter().AddGroup(parentRegularGroupId, propagationKind: Propagate.None);

                // act
                questionnaire.NewAddGroup(Guid.NewGuid(), parentRegularGroupId, "Title", Propagate.None, null, null);

                // assert
                Assert.That(GetSingleEvent<NewGroupAdded>(eventContext).ParentGroupPublicKey, Is.EqualTo(parentRegularGroupId));
            }
        }

        [Test]
        public void NewAddGroup_When_groups_propagation_kind_is_unsupported_Then_throws_DomainException()
        {
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            var unsupportedPropagationKing = Propagate.Propagated;

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), null, "Title", unsupportedPropagationKing, null, null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.NotSupportedPropagationGroup));
        }

        [TestCase(Propagate.None)]
        [TestCase(Propagate.AutoPropagated)]
        public void NewAddGroup_When_groups_propagation_kind_is_supported_Then_raised_NewAddGroup_event_contains_the_same_propagation_kind(Propagate supportedPopagationKind)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();

                // act
                questionnaire.NewAddGroup(Guid.NewGuid(), null, "Title", supportedPopagationKind, null, null);

                // assert
                Assert.That(GetSingleEvent<NewGroupAdded>(eventContext).Paropagateble, Is.EqualTo(supportedPopagationKind));
            }
        }

        [Test]
        public void UpdateGroup_When_group_does_not_exist_Then_throws_DomainException()
        {
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            Guid notExistingGroupPublicKey = Guid.NewGuid();

            // act
            TestDelegate act = () =>
            {
                questionnaire.NewUpdateGroup(notExistingGroupPublicKey, null, Propagate.None, null, null);
            };

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.GroupNotFound));
        }

        [Test]
        public void UpdateGroup_When_group_exists_Then_raised_GroupUpdated_event_contains_questionnaire_id()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var questionnaireId = Guid.NewGuid();
                var existingGroupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(questionnaireId, existingGroupPublicKey);

                // act
                questionnaire.NewUpdateGroup(existingGroupPublicKey, "Title", Propagate.None, null, null);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).QuestionnaireId, Is.EqualTo(questionnaireId.ToString()));
            }
        }

        [Test]
        public void UpdateGroup_When_group_exists_Then_raised_GroupUpdated_event_contains_group_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "group text", Propagate.None, null, null);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).GroupPublicKey, Is.EqualTo(groupPublicKey));
            }
        }

        [Test]
        public void UpdateGroup_When_group_exists_and_group_text_specified_Then_raised_GroupUpdated_event_with_same_group_text()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);
                var groupText = "new group text";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, groupText, Propagate.None, null, null);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).GroupText, Is.EqualTo(groupText));
            }
        }

        [Test]
        public void UpdateGroup_When_group_exists_and_propogatability_specified_Then_raised_GroupUpdated_event_with_same_propogatability()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);
                var propagatability = Propagate.AutoPropagated;

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "new text", propagatability, null, null);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).Propagateble, Is.EqualTo(propagatability));
            }
        }

        [Test]
        public void UpdateGroup_When_group_exists_and_condition_expression_specified_Then_raised_GroupUpdated_event_with_same_condition_expression()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);
                var conditionExpression = "2 < 7";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "text of a group", Propagate.None, null, conditionExpression);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).ConditionExpression, Is.EqualTo(conditionExpression));
            }
        }

        [Test]
        public void UpdateGroup_When_group_exists_and_description_specified_Then_raised_GroupUpdated_event_with_same_description()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);
                var description = "hardest questionnaire in the world";

                // act
                questionnaire.NewUpdateGroup(groupPublicKey, "Title", Propagate.None, description, null);

                // assert
                Assert.That(GetSingleEvent<GroupUpdated>(eventContext).Description, Is.EqualTo(description));
            }
        }

        [Test]
        public void ctor_When_public_key_specified_Then_raised_NewQuestionnaireCreated_event_with_same_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var publicKey = Guid.NewGuid();

                // act
                new QuestionnaireAR(publicKey, "title");

                // assert
                Assert.That(GetSingleEvent<NewQuestionnaireCreated>(eventContext).PublicKey, Is.EqualTo(publicKey));
            }
        }

        [Test]
        public void ctor_When_title_specified_Then_raised_NewQuestionnaireCreated_event_with_same_title()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var title = "title, the";

                // act
                new QuestionnaireAR(Guid.NewGuid(), title);

                // assert
                Assert.That(GetSingleEvent<NewQuestionnaireCreated>(eventContext).Title, Is.EqualTo(title));
            }
        }

        [Test]
        public void ctor_When_called_Then_raised_NewQuestionnaireCreated_event_with_creation_date_equal_to_current_date()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var currentDate = new DateTime(2010, 10, 20, 17, 00, 00);
                var clockStub = Mock.Of<IClock>(clock
                    => clock.UtcNow() == currentDate);
                NcqrsEnvironment.SetDefault(clockStub);

                // act
                new QuestionnaireAR(Guid.NewGuid(), "some title");

                // assert
                Assert.That(GetSingleEvent<NewQuestionnaireCreated>(eventContext).CreationDate, Is.EqualTo(currentDate));
            }
        }

        private static T GetSingleEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Single(e => e.Payload is T).Payload;
        }

        private static QuestionnaireAR CreateQuestionnaireAR()
        {
            return new QuestionnaireAR(Guid.NewGuid());
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneQuestion(Guid questionId)
        {
            return CreateQuestionnaireARWithOneGroupAndQuestionInIt(questionId);
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneQuestionnInTypeAndOptions(Guid questionId, QuestionType questionType, Option[] options)
        {
            return CreateQuestionnaireARWithOneGroupAndQuestionInIt(questionId, questionType: questionType, options: options);
        }

        private static QuestionnaireAR CreateQuestionnaireAR(Guid? questionnaireId = null, string text = "text of questionnaire")
        {
            return new QuestionnaireAR(questionnaireId ?? Guid.NewGuid(), text);
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneQuestionAndOneImage(Guid questionKey, Guid imageKey)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroupAndQuestionInIt(questionKey);

            questionnaire.UploadImage(questionKey, "image title", "image description", imageKey);

            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneGroup(Guid? questionnaireId = null, Guid? groupId = null, Propagate propagationKind = Propagate.None)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR(questionnaireId ?? Guid.NewGuid(), "Title");

            questionnaire.NewAddGroup(groupId ?? Guid.NewGuid(), null, "New group", propagationKind, null, null);

            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneAutoPropagatedGroup(Guid groupId)
        {
            return CreateQuestionnaireARWithOneGroup(groupId: groupId, propagationKind: Propagate.AutoPropagated);
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneNonPropagatedGroup(Guid groupId)
        {
            return CreateQuestionnaireARWithOneGroup(groupId: groupId, propagationKind: Propagate.None);
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneAutoGroupAndQuestionInIt(Guid questionId)
        {
            return CreateQuestionnaireARWithOneGroupAndQuestionInIt(
                questionId: questionId, groupPropagationKind: Propagate.AutoPropagated);
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneGroupAndQuestionInIt(Guid questionId, Guid? groupId = null,
            Propagate groupPropagationKind = Propagate.None, QuestionType questionType = QuestionType.Text, Option[] options = null)
        {
            groupId = groupId ?? Guid.NewGuid();

            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupId.Value, groupPropagationKind);

            questionnaire.NewAddQuestion(questionId,
                groupId.Value, "Title", questionType, "text", false, false,
                false, QuestionScope.Interviewer, "", "", "", "", options ?? new Option[] { }, Order.AsIs, null,
                new Guid[] { });

            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithTwoQuestions(Guid secondQuestionId)
        {
            var groupId = Guid.NewGuid();

            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupId);

            questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Title", QuestionType.Text, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);

            questionnaire.NewAddQuestion(secondQuestionId, groupId, "Title", QuestionType.Text, "name", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);

            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithTwoGroups(Guid firstGroup, Guid secondGroup)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneNonPropagatedGroup(firstGroup);

            questionnaire.NewAddGroup(secondGroup, null, "Second group", Propagate.None, null, null);

            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithAutoGroupAndRegularGroup(Guid autoGroupPublicKey, Guid secondGroup)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupPublicKey);

            questionnaire.NewAddGroup(secondGroup, null, "Second group", Propagate.None, null, null);

            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithAutoGroupAndRegularGroupAndQuestionInIt(Guid autoGroupPublicKey, Guid secondGroup, Guid autoQuestoinId)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithAutoGroupAndRegularGroup(autoGroupPublicKey, secondGroup);

            questionnaire.NewAddQuestion(autoQuestoinId, secondGroup, "Title", QuestionType.AutoPropagate, "auto", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);
            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithTwoRegularGroupsAndQuestionInLast(Guid firstGroup, Guid autoQuestoinId)
        {
            var secondGroup = Guid.NewGuid();
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithTwoGroups(firstGroup, secondGroup);
            questionnaire.NewAddQuestion(autoQuestoinId, secondGroup, "Title", QuestionType.AutoPropagate, "auto", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);
            return questionnaire;
        }


        #region [Answer option values allows only numbers]

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        #warning Roma: when part is incorrect should be something like when answer option value contains not number
        public void NewAddQuestion_When_answer_option_value_allows_only_numbers_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

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
        #warning Roma: when part is incorrect should be something like when answer option value contains not number
        public void NewUpdateQuestion_When_answer_option_value_allows_only_numbers_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);

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
        public void NewUpdateQuestion_When_answer_option_value_contains_only_numbers_Then_raised_QuestionChanged_event_contains_question_answer_with_only_numbers_value(
            QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid targetQuestionPublicKey = Guid.NewGuid();
                string answerValue = "10";
                var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);

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
                    options: new Option[1] { new Option(id: Guid.NewGuid(), title: "text", value: answerValue) });


                // assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Answers[0].AnswerValue, Is.EqualTo("10"));
            }
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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupId);

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
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Answers[0].AnswerValue, Is.EqualTo("10"));
            }
        }

        #endregion

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        [TestCase(QuestionType.Numeric)]
        [TestCase(QuestionType.Text)]
        [TestCase(QuestionType.DateTime)]
        [TestCase(QuestionType.AutoPropagate)]
        public void AddQuestion_When_question_type_is_allowed_Then_raised_NewQuestionAdded_event_with_same_question_type(
            QuestionType allowedQuestionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId);

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
                    options: AreOptionsRequiredByQuestionType(allowedQuestionType) ? CreateTwoOptions() : null);

                // assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).QuestionType, Is.EqualTo(allowedQuestionType));
            }
        }

        [Test]
        [TestCase(QuestionType.DropDownList)]
        [TestCase(QuestionType.GpsCoordinates)]
        [TestCase(QuestionType.YesNo)]
        public void AddQuestion_When_question_type_is_not_allowed_Then_DomainException_with_type_NotAllowedQuestionType_should_be_thrown(
            QuestionType notAllowedQuestionType)
        {
            // arrange
            Guid groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId);

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
        [TestCase(QuestionType.Numeric)]
        [TestCase(QuestionType.Text)]
        [TestCase(QuestionType.DateTime)]
        [TestCase(QuestionType.AutoPropagate)]
        public void UpdateQuestion_When_question_type_is_allowed_Then_raised_QuestionChanged_event_with_same_question_type(
            QuestionType allowedQuestionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid questionId = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestion(questionId);

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
        public void UpdateQuestion_When_question_type_is_not_allowed_Then_DomainException_with_type_NotAllowedQuestionType_should_be_thrown(
            QuestionType notAllowedQuestionType)
        {
            // arrange
            Guid questionId = Guid.NewGuid();
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestion(questionId);

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

        private static bool AreOptionsRequiredByQuestionType(QuestionType type)
        {
            return type == QuestionType.MultyOption || type == QuestionType.SingleOption;
        }

        private static Option[] CreateTwoOptions()
        {
            return new[]
            {
                new Option(Guid.Parse("00000000-1111-0000-1111-000000000000"), "-1", "No"),
                new Option(Guid.Parse("00000000-2222-0000-2222-000000000000"), "42", "Yes"),
            };
        }

        #region [Answer option value is required]

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void AddQuestion_When_answer_option_value_is_required_Then_DomainException_should_be_thrown(QuestionType questionTypeWithOptionsExpected)
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId);

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
        public void NewUpdateQuestion_When_answer_option_value_is_required_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);

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
                string answerValue = "10";
                var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);

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
                    options: new Option[1] { new Option(id: Guid.NewGuid(), title: "text", value: answerValue) });


                // assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Answers[0].AnswerValue, Is.EqualTo("10"));
            }
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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupId);

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
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Answers[0].AnswerValue, Is.EqualTo("10"));
            }
        }
        #endregion

        #region [Answer option values is unique in options scope]
        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_answer_option_values_not_unique_in_options_scope_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            var questionnaire = CreateQuestionnaireAR();

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
        public void NewUpdateQuestion_When_answer_option_values_not_unique_in_options_scope_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid targetQuestionPublicKey = Guid.NewGuid();
            var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);

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
                var questionnaire = CreateQuestionnaireARWithOneQuestion(targetQuestionPublicKey);

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
                Assert.That(
                    GetSingleEvent<QuestionChanged>(eventContext).Answers.Select(x => x.AnswerValue).Distinct().Count(),
                    Is.EqualTo(2));
            }
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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupId);

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
                Assert.That(
                    GetSingleEvent<NewQuestionAdded>(eventContext).Answers.Select(x => x.AnswerValue).Distinct().Count(),
                    Is.EqualTo(2));
            }
        }
        #endregion

        [Test]
        public void NewUpdateQuestion_when_question_is_AutoPropagate_and_list_of_triggers_is_null_then_rised_QuestionChanged_event_should_contains_null_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                var autoPropagateQuestionId = Guid.NewGuid();
                var autoPropagate = QuestionType.AutoPropagate;
                Guid[] emptyTriggedGroupIds = null;
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoGroupAndQuestionInIt(autoPropagateQuestionId);

                // Act
                questionnaire.NewUpdateQuestion(autoPropagateQuestionId, "What is your last name?", autoPropagate, "name", false, false,
                                               false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                               emptyTriggedGroupIds);


                // Assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Triggers, Is.Null);
            }
        }

        [Test]
        public void NewUpdateQuestion_when_question_is_AutoPropagate_and_list_of_triggers_is_empty_then_rised_QuestionChanged_event_should_contains_empty_list_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                var autoPropagateQuestionId = Guid.NewGuid();
                var autoPropagate = QuestionType.AutoPropagate;
                var emptyTriggedGroupIds = new Guid[0];
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoGroupAndQuestionInIt(autoPropagateQuestionId);

                // Act
                questionnaire.NewUpdateQuestion(autoPropagateQuestionId, "What is your last name?", autoPropagate, "name", false, false,
                                               false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                               emptyTriggedGroupIds);


                // Assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Triggers, Is.Empty);
            }
        }

        [Test]
        public void NewUpdateQuestion_when_question_is_AutoPropagate_and_list_of_triggers_contains_absent_group_id_then_DomainException_should_be_thrown()
        {
            // Arrange
            var autoPropagate = QuestionType.AutoPropagate;
            var autoPropagateQuestionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var absentGroupId = Guid.NewGuid();
            var triggedGroupIdsWithAbsentGroupId = new[] { absentGroupId };

            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroupAndQuestionInIt(autoPropagateQuestionId, groupId, questionType: QuestionType.AutoPropagate);

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(autoPropagateQuestionId, "What is your last name?", autoPropagate, "name", false, false,
                                               false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                               triggedGroupIdsWithAbsentGroupId);

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TriggerLinksToNotExistingGroup));
        }

        [Test]
        public void NewUpdateQuestion_when_question_is_AutoPropagate_and_list_of_triggers_contains_non_propagate_group_id_then_DomainException_should_be_thrown()
        {
            // Arrange
            var autoPropagate = QuestionType.AutoPropagate;
            var autoPropagateQuestionId = Guid.NewGuid();
            var nonPropagateGroupId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var triggedGroupIdsWithNonPropagateGroupId = new[] { nonPropagateGroupId };

            QuestionnaireAR questionnaire = CreateQuestionnaireARWithTwoRegularGroupsAndQuestionInLast(nonPropagateGroupId, autoPropagateQuestionId);

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(autoPropagateQuestionId, "What is your last name?", autoPropagate, "name", false, false,
                                               false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                               triggedGroupIdsWithNonPropagateGroupId);


            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TriggerLinksToNotPropagatedGroup));
        }

        [Test]
        public void NewUpdateQuestion_when_question_is_AutoPropagate_and_list_of_triggers_contains_propagate_group_id_then_rised_QuestionChanged_event_should_contains_that_group_id_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var autoPropagate = QuestionType.AutoPropagate;
                var autoPropagateQuestionId = Guid.NewGuid();
                var autoPropagateGroupId = Guid.NewGuid();
                var groupId = Guid.NewGuid();
                var triggedGroupIdsWithAutoPropagateGroupId = new[] { autoPropagateGroupId };

                QuestionnaireAR questionnaire = CreateQuestionnaireARWithAutoGroupAndRegularGroupAndQuestionInIt(autoPropagateGroupId, groupId, autoPropagateQuestionId);

                // Act
                questionnaire.NewUpdateQuestion(autoPropagateQuestionId, "What is your last name?", autoPropagate, "name", false, false,
                                               false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                               triggedGroupIdsWithAutoPropagateGroupId);

                // Assert
                Assert.That(GetSingleEvent<QuestionChanged>(eventContext).Triggers, Contains.Item(autoPropagateGroupId));
            }
        }

        [Test]
        public void NewAddQuestion_when_question_is_AutoPropagate_and_list_of_triggers_is_null_then_rised_NewQuestionAdded_event_should_contains_null_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                var autoPropagate = QuestionType.AutoPropagate;
                Guid[] emptyTriggedGroupIds = null;
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, emptyTriggedGroupIds);


                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Triggers, Is.Null);
            }
        }
        
        [Test]
        public void NewAddQuestion_when_question_is_AutoPropagate_and_list_of_triggers_is_empty_then_rised_NewQuestionAdded_event_should_contains_empty_list_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                var autoPropagate = QuestionType.AutoPropagate;
                var emptyTriggedGroupIds = new Guid[0];
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, emptyTriggedGroupIds);


                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Triggers, Is.Empty);
            }
        }
        
        [Test]
        public void NewAddQuestion_when_question_is_AutoPropagate_and_list_of_triggers_contains_absent_group_id_then_DomainException_of_type_TriggerLinksToNotExistingGroup_should_be_thrown()
        {
            // Arrange
            var autoPropagate = QuestionType.AutoPropagate;

            var groupId = Guid.NewGuid();
            var absentGroupId = Guid.NewGuid();
            var triggedGroupIdsWithAbsentGroupId = new[] { absentGroupId };

            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupId: groupId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                         false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                         string.Empty, new Option[0], Order.AZ, null, triggedGroupIdsWithAbsentGroupId);


            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TriggerLinksToNotExistingGroup));
        }
        
        [Test]
        public void NewAddQuestion_when_question_is_AutoPropagate_and_list_of_triggers_contains_non_propagate_group_id_then_DomainException_of_type_TriggerLinksToNotPropagatedGroup_should_be_thrown()
        {
            // Arrange
            var autoPropagate = QuestionType.AutoPropagate;
            var nonPropagateGroupId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var triggedGroupIdsWithNonPropagateGroupId = new[] { nonPropagateGroupId };

            QuestionnaireAR questionnaire = CreateQuestionnaireARWithTwoGroups(nonPropagateGroupId, groupId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                         false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                         string.Empty, new Option[0], Order.AZ, null, triggedGroupIdsWithNonPropagateGroupId);


            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TriggerLinksToNotPropagatedGroup));
        }

        [Test]
        public void NewAddQuestion_when_question_is_AutoPropagate_and_list_of_triggers_contains_propagate_group_id_then_rised_NewQuestionAdded_event_should_contains_that_group_id_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var autoPropagate = QuestionType.AutoPropagate;
                var autoPropagateGroupId = Guid.NewGuid();
                var groupId = Guid.NewGuid();
                var triggedGroupIdsWithAutoPropagateGroupId = new[] { autoPropagateGroupId };

                QuestionnaireAR questionnaire = CreateQuestionnaireARWithAutoGroupAndRegularGroup(autoPropagateGroupId, groupId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false,
                                             false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty,
                                             string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null,
                                             triggedGroupIdsWithAutoPropagateGroupId);

                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Triggers, Contains.Item(autoPropagateGroupId));
            }
        }


    }
}