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

        [Test]
        public void QuestionnaireARConstructor_When_questionnaire_title_is_not_empty_Then_raised_NewQuestionnaireCreated_event_contains_questionnaire_title()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var nonEmptyTitle = "Title";

                // act
                new QuestionnaireAR(Guid.NewGuid(), nonEmptyTitle, Guid.NewGuid());

                // assert
                Assert.That(GetSingleEvent<NewQuestionnaireCreated>(eventContext).Title, Is.EqualTo(nonEmptyTitle));
            }
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

        #region AddQuestion tests
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
        public void AddQuestion_When_stata_export_caption_has_33_chars_Then_DomainException_should_be_thrown()
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
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void AddQuestion_When_stata_export_caption_starts_with_digit_Then_DomainException_should_be_thrown()
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
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void AddQuestion_When_stata_export_caption_is_empty_Then_DomainException_should_be_thrown()
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
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void AddQuestion_When_stata_export_caption_contains_any_non_underscore_letter_or_digit_character_Then_DomainException_should_be_thrown()
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
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void AddQuestion_When_questionnaire_has_another_question_with_same_stata_export_caption_Then_DomainException_should_be_thrown()
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
            Assert.Throws<DomainException>(act);
        }

        #endregion

        #region ChangeQuestion tests

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
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestionPublicKey, "Title", new List<Guid>(), 0,"name", "", 
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
        public void ChangeQuestion_When_stata_export_caption_is_valid_Then_rised_QuestionChanged_event_contains_the_same_stata_caption(string validStataExportCaption)
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid targetQuestionPublicKey;
                var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

                // Act
                questionnaire.ChangeQuestion(targetQuestionPublicKey, "Title", new List<Guid>(), 0,
                                             validStataExportCaption, "", QuestionType.Text,
                                             QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                             Order.AZ, new Answer[0]);

                // Assert
                var risedEvent = GetSingleEvent<QuestionChanged>(eventContext);
                Assert.AreEqual(validStataExportCaption, risedEvent.StataExportCaption);
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
        public void ChangeQuestion_When_stata_export_caption_has_33_chars_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);
            string longStataExportCaption = "".PadRight(33, 'A');

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestionPublicKey, "Title", new List<Guid>(), 0,
                                                            longStataExportCaption, "",
                                                            QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                                            Order.AZ,
                                                            new Answer[0]);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void ChangeQuestion_When_stata_export_caption_starts_with_digit_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

            string stataExportCaptionWithFirstDigit = "1aaaa";

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestionPublicKey, "Title", new List<Guid>(), 0,
                                                            stataExportCaptionWithFirstDigit, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                                            Order.AZ, new Answer[0]);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void ChangeQuestion_When_stata_export_caption_has_trailing_spaces_and_is_valid_Then_rised_QuestionChanged_evend_should_contains_trimed_stata_caption()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid targetQuestionPublicKey;
                var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);
                string longStataExportCaption = " my_name38  ";

                // Act
                questionnaire.ChangeQuestion(targetQuestionPublicKey, "Title", new List<Guid>(), 0,
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
        public void ChangeQuestion_When_stata_export_caption_is_empty_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

            string emptyStataExportCaption = string.Empty;

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestionPublicKey, "Title", new List<Guid>(), 0,
                                                            emptyStataExportCaption, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                                            Order.AZ, new Answer[0]);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void ChangeQuestion_When_stata_export_caption_contains_any_non_underscore_letter_or_digit_character_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

            string nonValidStataExportCaptionWithBannedSymbols = "aaa:_&b";

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestionPublicKey, "Title", new List<Guid>(), 0,
                                                            nonValidStataExportCaptionWithBannedSymbols, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                                            Order.AZ, new Answer[0]);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Test]
        public void ChangeQuestion_When_questionnaire_has_another_question_with_same_stata_export_caption_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid targetQuestionPublicKey;
            var questionnaire = CreateQuestionnaireARWithOneQuestion(out targetQuestionPublicKey);

            questionnaire.AddQuestion(Guid.NewGuid(), "What is your first name?", "name", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);

            string duplicateStataExportCaption = "name";

            // Act
            TestDelegate act = () => questionnaire.ChangeQuestion(targetQuestionPublicKey, "What is your last name?", new List<Guid>(), 0,
                                                            duplicateStataExportCaption, "", QuestionType.Text,
                                                            QuestionScope.Interviewer, null, "", "", "", false, false, false,
                                                            Order.AZ, new Answer[0]);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        #endregion

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
                questionnaire.UpdateGroup(null, Propagate.None, existingGroupPublicKey, null, null, null);

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
                questionnaire.UpdateGroup(null, Propagate.None, groupPublicKey, null, null, description);

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
                new QuestionnaireAR(publicKey, null);

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
            return (T) eventContext.Events.Single(e => e.Payload is T).Payload;
        }

        private static QuestionnaireAR CreateQuestionnaireAR()
        {
            return new QuestionnaireAR();
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneQuestion(out Guid targetQuestionPublicKey)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            targetQuestionPublicKey = Guid.NewGuid();

            questionnaire.AddQuestion(targetQuestionPublicKey, "What is your last name?", "lastName", QuestionType.Text,
                                      QuestionScope.Interviewer,
                                      "", "", "", false, false, false, Order.AZ, "", null, new List<Guid>(), 0, new Answer[0]);
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
                new List<Guid>(), 0, new Answer[] {});

            questionnaire.UploadImage(questionKey, "image title", "image description", imageKey);

            return questionnaire;
        }

        private static QuestionnaireAR CreateQuestionnaireARWithOneGroup(Guid? questionnaireId = null, Guid? groupPublicKey = null)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR(questionnaireId ?? Guid.NewGuid(), null);

            questionnaire.AddGroup(groupPublicKey ?? Guid.NewGuid(), null, Propagate.None, null, null, null);

            return questionnaire;
        }
    }
}