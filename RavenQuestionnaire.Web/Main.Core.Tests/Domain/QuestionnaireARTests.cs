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
    [TestFixture]
    public class QuestionnaireARTests
    {
        #region AddQuestion tests

        [Test]
        public void AddQuestion_When_capital_parameter_is_true_Then_in_NewQuestionAdded_event_capital_property_should_be_set_in_true_too()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                bool caption = true;

                // Act
                questionnaire.AddQuestion(Guid.NewGuid(), "What is your last name?",
                                          "name", QuestionType.Text,
                                          QuestionScope.Interviewer,
                                          "", "", "", false, false, caption, Order.AZ, "", null, new List<Guid>(), 0,
                                          new Answer[0]);


                // Assert
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.AreEqual(caption, risedEvent.Capital);
            }
        }

        [TestCase("ma_name38")]
        [TestCase("__")]
        [TestCase("_123456789012345678901234567890_")]
        public void AddQuestion_When_stata_export_caption_is_valid_Then_rised_NewQuestionAdded_event_contains_the_same_stata_caption(string validStataExportCaption)
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();

                // Act
                questionnaire.AddQuestion(Guid.NewGuid(), "What is your last name?",
                                          validStataExportCaption, QuestionType.Text,
                                          QuestionScope.Interviewer,
                                          "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0,
                                          new Answer[0]);

                // Assert
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.AreEqual(validStataExportCaption, risedEvent.StataExportCaption);
            }
        }

        [Test]
        public void AddQuestion_When_stata_export_caption_has_trailing_spaces_and_is_valid_Then_rised_NewQuestionAdded_event_should_contains_trimed_stata_caption()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                string longStataExportCaption = " my_name38  ";

                // Act
                questionnaire.AddQuestion(Guid.NewGuid(), "What is your last name?",
                                          longStataExportCaption, QuestionType.Text,
                                          QuestionScope.Interviewer,
                                          "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0,
                                          new Answer[0]);

                // Assert
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.AreEqual(longStataExportCaption.Trim(), risedEvent.StataExportCaption);
            }
        }

        [Test]
        public void AddQuestion_When_stata_export_caption_has_33_chars_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            string longStataExportCaption = "".PadRight(33, 'A');

            // Act
            TestDelegate act = () => questionnaire.AddQuestion(Guid.NewGuid(), "What is your last name?",
                                                               longStataExportCaption, QuestionType.Text,
                                                               QuestionScope.Interviewer,
                                                               "", "", "", false, false, false, Order.AZ, "", null,
                                                               new List<Guid>(), 0, new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void AddQuestion_When_stata_export_caption_starts_with_digit_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            string stataExportCaptionWithFirstDigit = "1aaaa";

            // Act
            TestDelegate act = () => questionnaire.AddQuestion(Guid.NewGuid(), "What is your last name?",
                                                               stataExportCaptionWithFirstDigit, QuestionType.Text,
                                                               QuestionScope.Interviewer,
                                                               "", "", "", false, false, false, Order.AZ, "", null,
                                                               new List<Guid>(), 0, new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void
            AddQuestion_When_stata_export_caption_is_empty_contains_spaces_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            string emptyStataExportCaption = string.Empty;

            // Act
            TestDelegate act = () => questionnaire.AddQuestion(Guid.NewGuid(), "What is your last name?",
                                                               emptyStataExportCaption, QuestionType.Text,
                                                               QuestionScope.Interviewer,
                                                               "", "", "", false, false, false, Order.AZ, "", null,
                                                               new List<Guid>(), 0, new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void AddQuestion_When_stata_export_caption_contains_any_non_underscore_letter_or_digit_character_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            string nonValidStataExportCaptionWithBannedSymbols = "aaa:_&b";

            // Act
            TestDelegate act = () => questionnaire.AddQuestion(Guid.NewGuid(), "What is your last name?",
                                                               nonValidStataExportCaptionWithBannedSymbols,
                                                               QuestionType.Text,
                                                               QuestionScope.Interviewer,
                                                               "", "", "", false, false, false, Order.AZ, "", null,
                                                               new List<Guid>(), 0, new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void AddQuestion_When_questionnaire_has_another_question_with_same_stata_export_caption_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            questionnaire.AddQuestion(Guid.NewGuid(), "What is your first name?", "name", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            string duplicateStataExportCaption = "name";

            // Act
            TestDelegate act = () => questionnaire.AddQuestion(Guid.NewGuid(), "What is your last name?",
                                                               duplicateStataExportCaption, QuestionType.Text,
                                                               QuestionScope.Interviewer,
                                                               "", "", "", false, false, false, Order.AZ, "", null,
                                                               new List<Guid>(), 0, new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        #endregion

        #region ChangeQuestion tests

        [Test]
        public void ChangeQuestion_When_capital_parameter_is_true_Then_in_QuestionChanged_event_capital_property_should_be_set_in_true_too()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                Guid targetQuestion = Guid.NewGuid();

                questionnaire.AddQuestion(targetQuestion, "What is your last name?", "lastName", QuestionType.Text,
                                          QuestionScope.Interviewer,
                                          "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

                bool capital = true;

                // Act
                questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
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
        public void ChangeQuestion_When_stata_export_caption_is_valid_Then_rised_QuestionChanged_event_contains_the_same_stata_caption(string validStataExportCaption)
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                Guid targetQuestion = Guid.NewGuid();

                questionnaire.AddQuestion(targetQuestion, "What is your last name?", "lastName", QuestionType.Text,
                                          QuestionScope.Interviewer,
                                          "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

                // Act
                questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
                                             validStataExportCaption, "", QuestionType.Text,
                                             QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                             Order.AZ, new Answer[0]);

                // Assert
                var risedEvent = GetSingleEvent<QuestionChanged>(eventContext);
                Assert.AreEqual(validStataExportCaption, risedEvent.StataExportCaption);
            }
        }


        [Test]
        public void ChangeQuestion_When_we_updating_absent_question_Then_ArgumentException_should_be_thrown()
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
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void ChangeQuestion_When_stata_export_caption_has_33_chars_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            string longStataExportCaption = "".PadRight(33, 'A');
            Guid targetQuestion = Guid.NewGuid();

            questionnaire.AddQuestion(targetQuestion, "What is your last name?", "lastName", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
                                                            longStataExportCaption, "",
                                                            QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                                            Order.AZ,
                                                            new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void ChangeQuestion_When_stata_export_caption_starts_with_digit_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            Guid targetQuestion = Guid.NewGuid();

            questionnaire.AddQuestion(targetQuestion, "What is your last name?", "lastName", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            string stataExportCaptionWithFirstDigit = "1aaaa";

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
                                                            stataExportCaptionWithFirstDigit, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                                            Order.AZ, new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void ChangeQuestion_When_stata_export_caption_has_trailing_spaces_and_is_valid_Then_rised_QuestionChanged_evend_should_contains_trimed_stata_caption()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                QuestionnaireAR questionnaire = CreateQuestionnaireAR();
                string longStataExportCaption = " my_name38  ";
                Guid targetQuestion = Guid.NewGuid();

                questionnaire.AddQuestion(targetQuestion, "What is your last name?", "lastName", QuestionType.Text,
                                          QuestionScope.Interviewer,
                                          "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

                // Act
                questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
                                             longStataExportCaption, "",
                                             QuestionType.Text,
                                             QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                             Order.AZ,
                                             new Answer[0]);

                // Assert
                var risedEvent = GetSingleEvent<QuestionChanged>(eventContext);
                Assert.AreEqual(longStataExportCaption.Trim(), risedEvent.StataExportCaption);
            }
        }

        [Test]
        public void ChangeQuestion_When_stata_export_caption_is_empty_contains_spaces_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            Guid targetQuestion = Guid.NewGuid();

            questionnaire.AddQuestion(targetQuestion, "What is your last name?", "lastName", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            string emptyStataExportCaption = string.Empty;

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
                                                            emptyStataExportCaption, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                                            Order.AZ, new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void ChangeQuestion_When_stata_export_caption_contains_any_non_underscore_letter_or_digit_character_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            Guid targetQuestion = Guid.NewGuid();

            questionnaire.AddQuestion(targetQuestion, "What is your last name?", "lastName", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            string nonValidStataExportCaptionWithBannedSymbols = "aaa:_&b";

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
                                                            nonValidStataExportCaptionWithBannedSymbols, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                                            Order.AZ, new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void ChangeQuestion_When_questionnaire_has_another_question_with_same_stata_export_caption_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            Guid targetQuestion = Guid.NewGuid();

            questionnaire.AddQuestion(Guid.NewGuid(), "What is your first name?", "name", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            questionnaire.AddQuestion(targetQuestion, "What is your last name?", "lastName", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            string duplicateStataExportCaption = "name";

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestion, "What is your last name?", new List<Guid>(), 0,
                                                            duplicateStataExportCaption, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                                            Order.AZ, new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        #endregion

        private static T GetSingleEvent<T>(EventContext eventContext)
        {
            return (T) eventContext.Events.Single(e => e.Payload is T).Payload;
        }

        private static QuestionnaireAR CreateQuestionnaireAR()
        {
            return new QuestionnaireAR();
        }
    }
}