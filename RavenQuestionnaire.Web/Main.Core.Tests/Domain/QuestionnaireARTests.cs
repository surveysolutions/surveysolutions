using System;
using System.Collections.Generic;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;

namespace Main.Core.Tests.Domain
{
    [TestFixture]
    public class QuestionnaireARTests
    {
        [Test]
        public void ChangeQuestion_When_stata_export_caption_has_33_chars_Then_ArgumentException_should_be_thrown()
        {
            // Arrange
            QuestionnaireAR questionnaire = CreateQuestionnaireAR();
            string longStataExportCaption = "".PadRight(33, 'A');

            // Act
            TestDelegate act = () =>
                questionnaire.ChangeQuestion(Guid.NewGuid(), "Title", new List<Guid>(), 0, longStataExportCaption, "",
                                             QuestionType.Text,
                                             QuestionScope.Interviewer, null, "", "", "", false, false, Order.AZ,
                                             new Answer[0]);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        private QuestionnaireAR CreateQuestionnaireAR()
        {
            return new QuestionnaireAR();
        }
    }
}