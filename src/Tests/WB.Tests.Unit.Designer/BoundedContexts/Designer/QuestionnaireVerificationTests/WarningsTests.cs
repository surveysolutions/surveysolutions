using Main.Core.Entities.Composite;
using NUnit.Framework;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class WarningsTests
    {
        [Test]
        public void no_gps_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                })
                .ExpectWarning("WB0211")
                .AndNoWarning("WB0264");

        [Test]
        public void gps_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.GpsCoordinateQuestion(),
                })
                .ExpectNoWarning("WB0211")
                .AndWarning("WB0264");

        [Test]
        public void no_barcode_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                })
                .ExpectNoWarning("WB0267");

        [Test]
        public void barcode_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.QRBarcodeQuestion(),
                })
                .ExpectWarning("WB0267");

        [Test]
        public void less_than_50_percent_questions_with_validations()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                    Create.Question(),
                    Create.Question(validationConditions: new[] { Create.ValidationCondition() }),
                })
                .ExpectWarning("WB0208");

        [Test]
        public void more_than_50_percent_questions_with_validations()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                    Create.Question(validationConditions: new[] { Create.ValidationCondition() }),
                    Create.Question(validationConditions: new[] { Create.ValidationCondition() }),
                })
                .ExpectNoWarning("WB0208");

        [Test]
        public void more_than_30_percent_questions_are_text()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.TextQuestion(),
                    Create.TextQuestion(),
                    Create.NumericIntegerQuestion(),
                    Create.NumericIntegerQuestion(),
                })
                .ExpectWarning("WB0265");

        [Test]
        public void less_than_30_percent_questions_are_text()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.TextQuestion(),
                    Create.NumericIntegerQuestion(),
                    Create.NumericIntegerQuestion(),
                    Create.NumericIntegerQuestion(),
                })
                .ExpectNoWarning("WB0265");

        [Test]
        public void questions_with_same_title()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Question(title: "Question"),
                    Create.Question(title: "Question"),
                })
                .ExpectWarning("WB0266");

        [Test]
        public void questions_with_different_titles()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Question(title: "Question 1"),
                    Create.Question(title: "Question 2"),
                })
                .ExpectNoWarning("WB0266");
    }
}