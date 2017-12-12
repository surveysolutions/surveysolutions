using Main.Core.Entities.Composite;
using NUnit.Framework;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    public class ExpressionVerificationsTests
    {
        [Test]
        public void when_Verify_Question_with_circle_referance_on_question_in_validation_condition()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Question(variable: "x", validationConditions: new[] { Create.ValidationCondition(expression: "y > 10") }),
                    Create.Question(variable: "y", enablementCondition: "x > 10"),
                })
                .ExpectError("WB0056");

        [Test]
        public void when_Verify_Question_with_circle_referance_on_variable_in_validation_condition()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Question(variable: "x", validationConditions: new[]
                    {
                        Create.ValidationCondition(expression: "z > 10"),
                        Create.ValidationCondition(expression: "y > 10")
                    }),
                    Create.Variable(variableName: "y", expression: "x > 10"),
                })
                .ExpectError("WB0056");

        [Test]
        public void when_Verify_Question_with_circle_referance_on_question_and_then_on_variable_in_validation_condition()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Question(variable: "x", validationConditions: new[] { Create.ValidationCondition(expression: "y > 10") }),
                    Create.Question(variable: "y", enablementCondition: "z > 10"),
                    Create.Variable(variableName: "z", expression: "x > 10"),
                })
                .ExpectError("WB0056");

        [Test]
        public void when_Verify_Question_with_no_circle_referance_on_variable_in_validation_condition()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Question(variable: "x", validationConditions: new[] { Create.ValidationCondition(expression: "z > 10") }),
                    Create.Variable(variableName: "y", expression: "x > 10"),
                })
                .ExpectNoError("WB0056");
    }
}