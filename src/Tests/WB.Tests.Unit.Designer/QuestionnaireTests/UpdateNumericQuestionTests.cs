using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.QuestionnaireTests
{
    [TestFixture]
    class UpdateNumericQuestionTests
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void When_update_numeric_int_question_without_special_values_with_special_values()
        {
            // arrange
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(Id.gA,
                Create.FixedRoster(Id.g1, fixedRosterTitles: new []{ Create.FixedRosterTitle(1, "A")}, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id.g2, "n1", options: Create.Options(Create.Option(1, "Hello"), Create.Option(2, "World")))
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

            TestDelegate act = () => questionnaire.UpdateNumericQuestion(command);

            // assert
            
        }

        [Test]
        public void When_update_numeric_question_with_special_values_with_new_special_values()
        {
            // arrange
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(Id.gA,
                Create.FixedRoster(Id.g1, fixedRosterTitles: new []{ Create.FixedRosterTitle(1, "A")}, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id.g2, "n1")
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
            TestDelegate act = () => questionnaire.UpdateNumericQuestion(command);

            // assert
            
        }

        [Test]
        public void When_update_text_question_to_numeric_with_special_values()
        {
            // arrange
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(Id.gA,
                Create.FixedRoster(Id.g1, fixedRosterTitles: new []{ Create.FixedRosterTitle(1, "A")}, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id.g2, "n1")
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
            TestDelegate act = () => questionnaire.UpdateNumericQuestion(command);

            // assert
            
        }
    }
}
