using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.QuestionnaireTests
{
    [TestFixture]
    class UpdateNumericQuestionTests
    {
        [Test]
        public void When_update_numeric_int_question_without_special_values_with_special_values()
        {
            // arrange
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(Id.gA,
                Create.FixedRoster(Id.g1, fixedRosterTitles: new []{ Create.FixedRosterTitle(1, "A")}, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id.g2, "n1", options: Create.Options(Create.Option(10, "Мама"), Create.Option(20, "Мыла"), Create.Option(30, "Раму")))
                }));
            Questionnaire questionnaire = Create.Questionnaire(responsible: Id.gF, document: questionnaireDocument);

            // act
            var command = Create.Command.UpdateNumericQuestion(
                questionnaireId: Id.gA, 
                questionId: Id.g2,
                responsibleId: Id.gF,
                title: "Title", 
                isPreFilled: false, 
                scope: QuestionScope.Interviewer, 
                isInteger: true, 
                useFormatting: false, 
                options: Create.Options(Create.Option(1, "Hello"), Create.Option(2, "World")));

            questionnaire.UpdateNumericQuestion(command);

            // assert
            var optins = questionnaire.QuestionnaireDocument.Find<INumericQuestion>(Id.g2)?.Answers;

            Assert.That(optins, Is.Not.Null);
            Assert.That(optins.Count, Is.EqualTo(2));
            Assert.That(optins[0].GetParsedValue(), Is.EqualTo(1));
            Assert.That(optins[0].AnswerText, Is.EqualTo("Hello"));
            Assert.That(optins[1].GetParsedValue(), Is.EqualTo(2));
            Assert.That(optins[1].AnswerText, Is.EqualTo("World"));
        }

        [Test]
        public void When_update_numeric_question_with_special_values_with_new_special_values()
        {
            // arrange
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(Id.gA,
                Create.Group(Id.g1, children: new IComposite[]
                {
                    Create.NumericRealQuestion(Id.g2, "n1")
                }));
            Questionnaire questionnaire = Create.Questionnaire(responsible: Id.gF, document: questionnaireDocument);

            // act
            var command = Create.Command.UpdateNumericQuestion(
                questionnaireId: Id.gA, 
                questionId: Id.g2,
                responsibleId: Id.gF,
                title: "Title", 
                isPreFilled: false, 
                scope: QuestionScope.Interviewer, 
                isInteger: true, 
                useFormatting: false, 
                options: Create.Options(Create.Option(1, "Hello"), Create.Option(2, "World")));
            questionnaire.UpdateNumericQuestion(command);

            // assert
            var optins = questionnaire.QuestionnaireDocument.Find<INumericQuestion>(Id.g2)?.Answers;

            Assert.That(optins, Is.Not.Null);
            Assert.That(optins.Count, Is.EqualTo(2));
            Assert.That(optins[0].GetParsedValue(), Is.EqualTo(1));
            Assert.That(optins[0].AnswerText, Is.EqualTo("Hello"));
            Assert.That(optins[1].GetParsedValue(), Is.EqualTo(2));
            Assert.That(optins[1].AnswerText, Is.EqualTo("World"));
        }

        [Test]
        public void When_update_text_question_to_numeric_with_special_values()
        {
            // arrange
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(Id.gA, Create.TextQuestion(Id.g2, "n1"));
            Questionnaire questionnaire = Create.Questionnaire(responsible: Id.gF, document: questionnaireDocument);

            // act
            var command = Create.Command.UpdateNumericQuestion(
                questionnaireId: Id.gA, 
                questionId: Id.g2,
                responsibleId: Id.gF,
                title: "Title", 
                isPreFilled: false, 
                scope: QuestionScope.Interviewer, 
                isInteger: true, 
                useFormatting: false, 
                options: Create.Options(Create.Option(1, "Hello"), Create.Option(2, "World")));
            questionnaire.UpdateNumericQuestion(command);

            // assert
            var optins = questionnaire.QuestionnaireDocument.Find<INumericQuestion>(Id.g2)?.Answers;

            Assert.That(optins, Is.Not.Null);
            Assert.That(optins.Count, Is.EqualTo(2));
            Assert.That(optins[0].GetParsedValue(), Is.EqualTo(1));
            Assert.That(optins[0].AnswerText, Is.EqualTo("Hello"));
            Assert.That(optins[1].GetParsedValue(), Is.EqualTo(2));
            Assert.That(optins[1].AnswerText, Is.EqualTo("World"));
        }
        
        [Test]
        public void When_update_question_in_deleted_questionnaire()
        {
            // arrange
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(Id.gA, Create.TextQuestion(Id.g2, "n1"));
            questionnaireDocument.IsDeleted = true;
            Questionnaire questionnaire = Create.Questionnaire(responsible: Id.gF, document: questionnaireDocument);

            // act
            var command = Create.Command.UpdateNumericQuestion(
                questionnaireId: Id.gA, 
                questionId: Id.g2,
                responsibleId: Id.gF,
                title: "Title", 
                isPreFilled: false, 
                scope: QuestionScope.Interviewer, 
                isInteger: true, 
                useFormatting: false, 
                options: Create.Options(Create.Option(1, "Hello"), Create.Option(2, "World")));
            

            // assert
            var exception = Assert.Catch<QuestionnaireException>(() => questionnaire.UpdateNumericQuestion(command));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.ErrorType, Is.EqualTo(DomainExceptionType.QuestionnaireIsDeleted));
        }
    }
}
