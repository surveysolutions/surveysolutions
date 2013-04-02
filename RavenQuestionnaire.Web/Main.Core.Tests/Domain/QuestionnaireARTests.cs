using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
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
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("      ")] /* contains \t symbol */
        public void QuestionnaireARConstructor_When_questionnaire_title_is_empty_or_contains_whitespaces_only_Then_throws_DomainException(string emptyTitle)
        {
            // arrange

            // act
            TestDelegate act = () => new QuestionnaireAR(Guid.NewGuid(), emptyTitle);

            // assert
            Assert.That(act, Throws.InstanceOf<DomainException>());
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("      ")] /* contains \t symbol */
        public void UpdateQuestionnaire_When_questionnaire_title_is_empty_or_contains_whitespaces_only_Then_throws_DomainException(string emptyTitle)
        {
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            // act
            TestDelegate act = () => questionnaire.UpdateQuestionnaire(emptyTitle);

            // assert
            Assert.That(act, Throws.InstanceOf<DomainException>());
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
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoGroup(autoGroupId);

            // Act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(Guid.NewGuid(), autoGroupId, "What is your last name?", QuestionType.Text,
                                             "name",
                                             false, isFeatured, false, QuestionScope.Interviewer, "", "", "", "",
                                             new Option[0], Order.AsIs, 0, new Guid[0]);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void NewAddQuestion_when_question_is_featured_and_inside_non_propagated_group_then_raised_NewQuestionAdded_event_contains_the_same_featured_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid groupId = Guid.NewGuid();
                bool isFeatured = true;
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupPublicKey: groupId);

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
        public void
            NewAddQuestion_When_Title_is_not_empty_Then_rised_Then_NewQuestionAdded_event_contains_the_same_title_caption
            ()
        {
            using (var eventContext = new EventContext())
            {
                var questionnaireKey = Guid.NewGuid();
                var groupKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(questionnaireKey, groupKey);

                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "not empty", QuestionType.Text, "test", false,
                                             false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty,
                                             string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, new Guid[0]);
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.AreEqual("not empty", risedEvent.QuestionText);
            }
        }

        [Test]
        public void NewUpdateQuestion_When_Title_is_empty_Then_QuestionChanged_event_contains_the_same_title_caption
            ()
        {
            using (var eventContext = new EventContext())
            {
                Guid questionKey;
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestion(out questionKey);

                questionnaire.NewUpdateQuestion(questionKey, "not empty", QuestionType.Text, "test", false, false,
                                                false, QuestionScope.Interviewer, string.Empty, string.Empty,
                                                string.Empty,
                                                string.Empty, new Option[0], Order.AZ, null, new Guid[0]);
                var risedEvent = GetSingleEvent<QuestionChanged>(eventContext);
                Assert.AreEqual("not empty", risedEvent.QuestionText);
            }
        }

        [Test]
        public void NewAddQuestion_When_Title_is_empty_Then_DomainException_should_be_thrown
            ()
        {
            var questionnaireKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(questionnaireKey, groupKey);
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "", QuestionType.Text, "test", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, new Guid[0]);
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void NewUpdateQuestion_When_Title_is_empty_Then_DomainException_should_be_thrown
            ()
        {
            Guid questionKey;
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestion(out questionKey);
            TestDelegate act =
                () =>
                questionnaire.NewUpdateQuestion(questionKey, "", QuestionType.Text, "test", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, new Guid[0]);
            Assert.Throws<DomainException>(act);
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
            Assert.Throws<DomainException>(act);
            // assert
            
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
            Guid questionKey;
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestionnInType(out questionKey, questionType);
            Option[] options = new Option[1] {new Option(Guid.NewGuid(), "1", string.Empty)};
            // act
            TestDelegate act =
                () =>
                questionnaire.NewUpdateQuestion(questionKey, "test", questionType, "test", false, false, false,
                                                QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                string.Empty, options, Order.AsIs, null, new Guid[0]);
            Assert.Throws<DomainException>(act);
            // assert
            
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_AnswerTitleIsNotEmpty_Then_event_contains_the_same_answer_title(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                Guid questionKey;
                Option[] options = new Option[1] { new Option(Guid.NewGuid(), "1", "title") };
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestionnInTypeAndOptions(out questionKey,
                                                                                                      questionType,
                                                                                                      new Answer[1]
                                                                                                          {
                                                                                                              new Answer
                                                                                                          ()
                                                                                                                  {
                                                                                                                      AnswerText
                                                                                                                          = "t",
                                                                                                                      AnswerValue
                                                                                                                          = "1"
                                                                                                                  }
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

            Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "title"),new Option(Guid.NewGuid(), "2", "title") };

            // act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                         false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                         string.Empty, options, Order.AsIs, null, new Guid[0]);
            Assert.Throws<DomainException>(act);
            // assert
            
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
            Guid questionKey;
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestionnInType(out questionKey, questionType);
            Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "title"), new Option(Guid.NewGuid(), "2", "title") };
            // act
            TestDelegate act =
                () =>
                questionnaire.NewUpdateQuestion(questionKey, "test", questionType, "test", false, false, false,
                                                QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                string.Empty, options, Order.AsIs, null, new Guid[0]);
            Assert.Throws<DomainException>(act);
            // assert

        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewUpdateQuestion_When_AnswerTitleIsUnique_Then_event_contains_the_same_answer_titles(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                Guid questionKey;
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneQuestionnInType(out questionKey, questionType);
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
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupPublicKey: groupId);

            // Act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                                             "name", false, false,
                                             isHeadOfPropagatedGroup,
                                             QuestionScope.Interviewer, "", "", "", "",
                                             new Option[0], Order.AsIs, 0, new Guid[0]);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void NewAddQuestion_when_question_is_head_of_propagated_group_and_inside_propagated_group_then_raised_NewQuestionAdded_event_contains_the_same_header_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid autoGroupId = Guid.NewGuid();
                bool isHeadOfPropagatedGroup = true;
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoGroup(autoGroupId);

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
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                bool capital = true;

                // Act
                questionnaire.AddQuestion(Guid.NewGuid(), "What is your last name?",
                                          "name", QuestionType.Text,
                                          QuestionScope.Interviewer,
                                          "", "", "", false, false, capital, Order.AZ, "", null, new List<Guid>(), 0,
                                          new Answer[0]);


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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupPublicKey: groupId);

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

            // Assert
            Assert.Throws<DomainException>(act);
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

            // Assert
            Assert.Throws<DomainException>(act);
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


            // Assert
            Assert.Throws<DomainException>(act);
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

            // Assert
            Assert.Throws<DomainException>(act);
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


            // Assert
            Assert.Throws<DomainException>(act);
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

            // Assert
            Assert.Throws<DomainException>(act);
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

            // Assert
            Assert.Throws<DomainException>(act);
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
            var emptyAnswersList = new Answer[0];

            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            // Act
            TestDelegate act = () => questionnaire.AddQuestion(Guid.NewGuid(), "What is your last name?", "name",
                                                               questionType,
                                                               QuestionScope.Interviewer, "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0,
                                                               emptyAnswersList);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void ChangeQuestion_When_QuestionType_is_option_type_and_answer_options_list_is_empty_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // Arrange
            var emptyAnswersList = new Answer[0];

            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestionPublicKey, "Title", new List<Guid>(), 0, "name", "",
                                             questionType,
                                             QuestionScope.Interviewer, null, "", "", "", false, false, false, Order.AZ,
                                             emptyAnswersList);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void ChangeQuestion_When_capital_parameter_is_true_Then_in_QuestionChanged_event_capital_property_should_be_set_in_true_too()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid targetQuestionPublicKey;
                var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

                bool capital = true;

                // Act
                questionnaire.ChangeQuestion(targetQuestionPublicKey, "Title", new List<Guid>(), 0,
                                             "title", "", QuestionType.Text,
                                             QuestionScope.Interviewer, null, "", "", "", false, false, capital,
                                             Order.AZ, new Answer[0]);


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
                Guid targetQuestionPublicKey;
                var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

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
            TestDelegate act = () => questionnaire.ChangeQuestion(Guid.NewGuid(), "Title", new List<Guid>(), 0,
                                                            "valid", "",
                                                            QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                                            Order.AZ,
                                                            new Answer[0]);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void NewUpdateQuestion_When_variable_name_has_33_chars_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);
            string longVariableName = "".PadRight(33, 'A');

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text,
                                                longVariableName,
                                                false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                new Option[0], Order.AZ, 0, new Guid[0]);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void NewUpdateQuestion_When_variable_name_starts_with_digit_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

            string stataExportCaptionWithFirstDigit = "1aaaa";

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text,
                                                stataExportCaptionWithFirstDigit,
                                                false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                new Option[0], Order.AZ, 0, new Guid[0]);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void NewUpdateQuestion_When_variable_name_has_trailing_spaces_and_is_valid_Then_rised_QuestionChanged_evend_should_contains_trimed_stata_caption()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid targetQuestionPublicKey;
                var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);
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
            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

            string emptyVariableName = string.Empty;

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text,
                                                emptyVariableName,
                                                false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                new Option[0], Order.AZ, 0, new Guid[0]);
            

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void NewUpdateQuestion_When_variable_name_contains_any_non_underscore_letter_or_digit_character_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

            string nonValidVariableNameWithBannedSymbols = "aaa:_&b";

            // Act
            TestDelegate act = () => questionnaire.NewUpdateQuestion(targetQuestionPublicKey, "Title", QuestionType.Text,
                                                nonValidVariableNameWithBannedSymbols,
                                                false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                new Option[0], Order.AZ, 0, new Guid[0]);

            // Assert
            Assert.Throws<DomainException>(act);
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

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void DeleteGroup_When_group_public_key_specified_Then_raised_GroupDeleted_event_with_same_group_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                Guid groupPublicKey = Guid.NewGuid();

                // act
                questionnaire.DeleteGroup(groupPublicKey, Guid.NewGuid());

                // assert
                Assert.That(GetSingleEvent<GroupDeleted>(eventContext).GroupPublicKey, Is.EqualTo(groupPublicKey));
            }
        }

        [Test]
        public void DeleteGroup_When_parent_element_public_key_specified_Then_raised_GroupDeleted_event_with_same_parent_element_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                Guid parentElementPublicKey = Guid.NewGuid();

                // act
                questionnaire.DeleteGroup(Guid.NewGuid(), parentElementPublicKey);

                // assert
                Assert.That(GetSingleEvent<GroupDeleted>(eventContext).ParentPublicKey, Is.EqualTo(parentElementPublicKey));
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
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                var questionId = Guid.NewGuid();

                // act
                questionnaire.DeleteQuestion(questionId, Guid.NewGuid());

                // assert
                Assert.That(GetSingleEvent<QuestionDeleted>(eventContext).QuestionId, Is.EqualTo(questionId));
            }
        }

        [Test]
        public void DeleteQuestion_When_parent_element_public_key_specified_Then_raised_QuestionDeleted_event_with_same_parent_element_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                var parentPublicKey = Guid.NewGuid();

                // act
                questionnaire.DeleteQuestion(Guid.NewGuid(), parentPublicKey);

                // assert
                Assert.That(GetSingleEvent<QuestionDeleted>(eventContext).ParentPublicKey, Is.EqualTo(parentPublicKey));
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

        [Test]
        public void NewUpdateGroup_When_groups_new_title_is_empty_Then_throws_DomainException()
        {
            // arrange
            var groupPublicKey = Guid.NewGuid();
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupPublicKey);
            string emptyTitle = string.Empty;

            // act
            TestDelegate act = () => questionnaire.NewUpdateGroup(groupPublicKey, emptyTitle, Propagate.None, null, null);

            // assert
            Assert.That(act, Throws.InstanceOf<DomainException>());
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
            Assert.That(act, Throws.InstanceOf<DomainException>());
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

        [Test]
        public void NewAddGroup_When_groups_title_is_empty_Then_throws_DomainException()
        {
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            string emptyTitle = string.Empty;

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), null, emptyTitle, Propagate.None, null, null);

            // assert
            Assert.That(act, Throws.InstanceOf<DomainException>());
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
        public void NewAddGroup_When_groups_propagation_kind_is_unsupported_Then_throws_DomainException()
        {
            // arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            var unsupportedPropagationKing = Propagate.Propagated;

            // act
            TestDelegate act = () => questionnaire.NewAddGroup(Guid.NewGuid(), null, "Title", unsupportedPropagationKing, null, null);

            // assert
            Assert.That(act, Throws.InstanceOf<DomainException>());
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
                questionnaire.UpdateGroup(null, Propagate.None, notExistingGroupPublicKey, null, null, null);

            // assert
            Assert.That(act, Throws.InstanceOf<DomainException>());
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
                questionnaire.UpdateGroup("Title", Propagate.None, existingGroupPublicKey, null, null, null);

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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupPublicKey: groupPublicKey);

                // act
                questionnaire.UpdateGroup("group text", Propagate.None, groupPublicKey, null, null, null);

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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupPublicKey: groupPublicKey);
                var groupText = "new group text";

                // act
                questionnaire.UpdateGroup(groupText, Propagate.None, groupPublicKey, null, null, null);

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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupPublicKey: groupPublicKey);
                var propagatability = Propagate.AutoPropagated;

                // act
                questionnaire.UpdateGroup("new text", propagatability, groupPublicKey, null, null, null);

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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupPublicKey: groupPublicKey);
                var conditionExpression = "2 < 7";

                // act
                questionnaire.UpdateGroup("text of a group", Propagate.None, groupPublicKey, null, conditionExpression, null);

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
                QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(groupPublicKey: groupPublicKey);
                var description = "hardest questionnaire in the world";

                // act
                questionnaire.UpdateGroup("Title", Propagate.None, groupPublicKey, null, null, description);

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
            return new QuestionnaireAR();
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneQuestion(out Guid targetQuestionPublicKey)
        {
            return CreateQuestionnaireARWithOneQuestionnInType(out targetQuestionPublicKey, QuestionType.Text);
        }
        private static QuestionnaireAR CreateQuestionnaireARWithOneQuestionnInType(out Guid targetQuestionPublicKey, QuestionType questionType)
        {
            return CreateQuestionnaireARWithOneQuestionnInTypeAndOptions(out targetQuestionPublicKey, QuestionType.Text, new Answer[0]);
        }
        private static QuestionnaireAR CreateQuestionnaireARWithOneQuestionnInTypeAndOptions(out Guid targetQuestionPublicKey, QuestionType questionType, Answer[] options)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            targetQuestionPublicKey = Guid.NewGuid();

            questionnaire.AddQuestion(targetQuestionPublicKey, "What is your last name?", "lastName", questionType,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, options);
            return questionnaire;
        }
        private static QuestionnaireAR CreateQuestionnaireAR(Guid? questionnaireId = null, string text = "text of questionnaire")
        {
            return new QuestionnaireAR(questionnaireId ?? Guid.NewGuid(), text);
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneQuestionAndOneImage(Guid questionKey, Guid imageKey)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            questionnaire.AddQuestion(questionKey, "What is your middle name?", "middlename",
                QuestionType.Text, QuestionScope.Interviewer, null, null, null,
                false, false, false, Order.AZ, null, null,
                new List<Guid>(), 0, new Answer[] { });

            questionnaire.UploadImage(questionKey, "image title", "image description", imageKey);

            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneGroup(Guid? questionnaireId = null, Guid? groupPublicKey = null)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR(questionnaireId ?? Guid.NewGuid(), "Title");

            questionnaire.AddGroup(groupPublicKey ?? Guid.NewGuid(), "New group", Propagate.None, null, null, null);

            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneAutoGroup(Guid autoGroupId)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR(Guid.NewGuid(), "Title");

            questionnaire.AddGroup(autoGroupId, "New auto group", Propagate.AutoPropagated, null, null, null);

            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneAutoGroupAndQuestionInIt(Guid questionId)
        {
            var autoGroupId = Guid.NewGuid();

            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoGroup(autoGroupId);

            questionnaire.NewAddQuestion(questionId, autoGroupId, "Title", QuestionType.Text, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);

            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneGroupAndQuestionInIt(Guid questionId, Guid? groupPublicKey = null)
        {
            var groupId = groupPublicKey ?? Guid.NewGuid();

            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupId);

            questionnaire.NewAddQuestion(questionId, groupId, "Title", QuestionType.Text, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);

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

        [Test]
        public void AddQuestion_When_answer_multi_option_value_allows_only_numbers_Then_DomainException_should_be_thrown()
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            // Act
            TestDelegate act =
                () =>
                questionnaire.AddQuestion(
                    publicKey: Guid.NewGuid(),
                    questionText: "What is your last name?",
                    stataExportCaption: "name",
                    questionType: QuestionType.MultyOption,
                    questionScope: QuestionScope.Interviewer,
                    conditionExpression: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    featured: false,
                    mandatory: false,
                    capital: false,
                    answerOrder: Order.AZ,
                    instructions: string.Empty,
                    groupPublicKey: null,
                    triggers: new List<Guid>(),
                    maxValue: 0,
                    answers:
                    new Answer[1]
                        {
                            new Answer()
                                {
                                    PublicKey = Guid.NewGuid(),
                                    AnswerType = AnswerType.Select,
                                    AnswerValue = "some text",
                                    AnswerText = "text"
                                }
                        });

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.Message, Is.StringContaining("only number characters"));
        }

        [Test]
        public void AddQuestion_When_answer_single_option_value_allows_only_numbers_Then_DomainException_should_be_thrown()
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            // Act
            TestDelegate act =
                () =>
                questionnaire.AddQuestion(
                    publicKey: Guid.NewGuid(),
                    questionText: "What is your last name?",
                    stataExportCaption: "name",
                    questionType: QuestionType.SingleOption,
                    questionScope: QuestionScope.Interviewer,
                    conditionExpression: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    featured: false,
                    mandatory: false,
                    capital: false,
                    answerOrder: Order.AZ,
                    instructions: string.Empty,
                    groupPublicKey: null,
                    triggers: new List<Guid>(),
                    maxValue: 0,
                    answers:
                    new Answer[1]
                        {
                            new Answer()
                                {
                                    PublicKey = Guid.NewGuid(),
                                    AnswerType = AnswerType.Select,
                                    AnswerValue = "some text",
                                    AnswerText = "text"
                                }
                        });

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.Message, Is.StringContaining("only number characters"));
        }

        [Test]
        public void
            NewAddQuestion_When_answer_multi_option_value_allows_only_numbers_Then_DomainException_should_be_thrown()
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            // Act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: Guid.NewGuid(),
                    title: "What is your last name?",
                    type: QuestionType.MultyOption,
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
            Assert.That(domainException.Message, Is.StringContaining("only number characters"));
        }

        [Test]
        public void
            NewAddQuestion_When_answer_single_option_value_allows_only_numbers_Then_DomainException_should_be_thrown()
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            // Act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: Guid.NewGuid(),
                    title: "What is your last name?",
                    type: QuestionType.SingleOption,
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
            Assert.That(domainException.Message, Is.StringContaining("only number characters"));
        }

        [Test]
        public void ChangeQuestion_When_answer_multi_option_value_allows_only_numbers_Then_DomainException_should_be_thrown()
        {
            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

            // Act
            TestDelegate act =
                () =>
                questionnaire.ChangeQuestion(
                    publicKey: targetQuestionPublicKey,
                    questionText: "What is your last name?",
                    stataExportCaption: "name",
                    questionType: QuestionType.MultyOption,
                    questionScope: QuestionScope.Interviewer,
                    conditionExpression: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    featured: false,
                    mandatory: false,
                    capital: false,
                    answerOrder: Order.AZ,
                    instructions: string.Empty,
                    groupPublicKey: null,
                    triggers: new List<Guid>(),
                    maxValue: 0,
                    answers:
                    new Answer[1]
                        {
                            new Answer()
                                {
                                    PublicKey = Guid.NewGuid(),
                                    AnswerType = AnswerType.Select,
                                    AnswerValue = "some text",
                                    AnswerText = "text"
                                }
                        });

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.Message, Is.StringContaining("only number characters"));
        }

        [Test]
        public void ChangeQuestion_When_answer_single_option_value_allows_only_numbers_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

            // Act
            TestDelegate act =
                () =>
                questionnaire.ChangeQuestion(
                    publicKey: targetQuestionPublicKey,
                    questionText: "What is your last name?",
                    stataExportCaption: "name",
                    questionType: QuestionType.SingleOption,
                    questionScope: QuestionScope.Interviewer,
                    conditionExpression: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    featured: false,
                    mandatory: false,
                    capital: false,
                    answerOrder: Order.AZ,
                    instructions: string.Empty,
                    groupPublicKey: null,
                    triggers: new List<Guid>(),
                    maxValue: 0,
                    answers:
                    new Answer[1]
                        {
                            new Answer()
                                {
                                    PublicKey = Guid.NewGuid(),
                                    AnswerType = AnswerType.Select,
                                    AnswerValue = "some text",
                                    AnswerText = "text"
                                }
                        });

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.Message, Is.StringContaining("only number characters"));
        }
    }
}