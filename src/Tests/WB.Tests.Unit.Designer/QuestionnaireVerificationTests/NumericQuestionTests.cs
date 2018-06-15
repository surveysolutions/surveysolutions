using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    public class NumericQuestionTests
    {
        [TestCase("non-integer")]
        [TestCase("3.5")]
        [TestCase("9999999999999999")]
        public void special_values_has_not_integer_values(string nonIntegerValue)
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id.g1, options: new Option[]{
                        new Option(nonIntegerValue, "Option text")
                    })
                })
                .ExpectError("WB0131");

        [Test]
        public void special_values_has_option_with_long_text()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id.g1, options: Create.Options(
                        Create.Option(1, "A".PadRight(251, 'A'))
                    ))
                })
                .ExpectError("WB0132");

        [Test]
        public void special_values_have_not_unique_values()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id.g1, options: Create.Options(
                        Create.Option(1, "A"),
                        Create.Option(2, "B"),
                        Create.Option(1, "C")
                    ))
                })
                .ExpectError("WB0133");

        [Test]
        public void special_values_count_is_more_than_allowed()
        {
            var options = Enumerable.Range(1, 201).Select(x => Create.Option(x, $"option {x}")).ToArray();
            Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id.g1, options: Create.Options(options))
                })
                .ExpectError("WB0134");
        }

        [Test]
        public void special_values_for_roster_size_cant_exceed_roster_maximum()
        {
            Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id.g1, options: Create.Options(
                        Create.Option(1, "A"),
                        Create.Option(2, "B"),
                        Create.Option(61, "C")
                    )),
                    Create.NumericRoster(Id.g2, rosterSizeQuestionId: Id.g1, children: new IComposite[]
                    {
                        Create.NumericRoster(Id.g3, rosterSizeQuestionId: Id.g1)
                    })
                })
                .ExpectError("WB0135");
        }

        [Test]
        public void special_values_for_roster_size_cant_exceed_long_roster_maximum()
        {
            Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id.g1, options: Create.Options(
                        Create.Option(1, "A"),
                        Create.Option(2, "B"),
                        Create.Option(201, "C")
                    )),
                    Create.NumericRoster(Id.g2, rosterSizeQuestionId: Id.g1, children: new IComposite[]
                    {
                        Create.TextQuestion(Id.g3)
                    })
                })
                .ExpectError("WB0135");
        }

        [Test]
        public void special_values_has_empty_values()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(Id.g1, options: Create.Options(
                    new Option(string.Empty, "A")
                ))
            )
            .ExpectError("WB0136");
        }

        [Test]
        public void special_values_has_not_unique_titles()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(Id.g1, options: Create.Options(
                        Create.Option(1, "A"),
                        Create.Option(2, "B"),
                        Create.Option(3, "  B  ")
                    ))
                )
                .ExpectError("WB0137");
        }
    }
}