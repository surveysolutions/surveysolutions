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

namespace Main.Core.Tests.Domain
{
    [TestFixture]
    public class QuestionnaireARTests
    {
        [Test]
        public void ChangeQuestion_When_we_updating_absent_question_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(Guid.NewGuid(), "Title", new List<Guid>(), 0,
                                                            "valid", "",
                                                            QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false,
                                                            Order.AZ,
                                                            new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void ChangeQuestion_When_stata_export_caption_has_trailing_spaces_and_is_valid_Then_rised_evend_contains_trimed_stata_caption()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            string longStataExportCaption = " my_name38  ";
            Guid targetQuestion = Guid.NewGuid();

            questionnaire.AddQuestion(targetQuestion, "What is your last name?", "lastName", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            QuestionChanged risedEvent = null;

            // Act
            using (var ctx = new EventContext())
            {
                questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
                                             longStataExportCaption, "",
                                             QuestionType.Text,
                                             QuestionScope.Interviewer, null, "", "", "", false, false,
                                             Order.AZ,
                                             new Answer[0]);

                foreach (UncommittedEvent item in ctx.Events)
                {
                    risedEvent = item.Payload as QuestionChanged;
                    if (risedEvent != null)
                    {
                        continue;
                    }
                    break;
                }
            }

            // Assert
            Assert.AreEqual(longStataExportCaption.Trim(), risedEvent.StataExportCaption);
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
                                      "", "", "", false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
                                                            longStataExportCaption, "",
                                                            QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false,
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
                                      "", "", "", false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            string stataExportCaptionWithFirstDigit = "1aaaa";

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
                                                            stataExportCaptionWithFirstDigit, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false,
                                                            Order.AZ, new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void ChangeQuestion_When_stata_export_caption_is_empty_contains_spaces_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            Guid targetQuestion = Guid.NewGuid();

            questionnaire.AddQuestion(targetQuestion, "What is your last name?", "lastName", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            string emptyStataExportCaption = string.Empty;

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
                                                            emptyStataExportCaption, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false,
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
                                      "", "", "", false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            string nonValidStataExportCaptionWithBannedSymbols = "aaa:_&b";

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestion, "Title", new List<Guid>(), 0,
                                                            nonValidStataExportCaptionWithBannedSymbols, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false,
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
                                      "", "", "", false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            questionnaire.AddQuestion(targetQuestion, "What is your last name?", "lastName", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            string duplicateStataExportCaption = "name";

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestion, "What is your last name?", new List<Guid>(), 0,
                                                            duplicateStataExportCaption, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false,
                                                            Order.AZ, new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        private QuestionnaireAR CreateQuestionnaireAR()
        {
            return new QuestionnaireAR();
        }
    }
}