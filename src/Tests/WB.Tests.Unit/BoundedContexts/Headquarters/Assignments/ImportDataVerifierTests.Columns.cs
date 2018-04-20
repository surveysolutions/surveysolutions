using System.Linq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class ImportDataVerifierTests
    {
        [Test]
        public void when_verify_columns_and_have_duplicated_columns_should_return_PL0031_error()
        {
            // arrange
            var variable = "textquestion";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion(variable: variable)}));

            var preloadedFile = Create.Entity.PreloadedFileInfo(new[] { variable, variable });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] {preloadedFile}, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(2));
            Assert.That(errors[0].Code, Is.EqualTo("PL0031"));
            Assert.That(errors[1].Code, Is.EqualTo("PL0031"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(variable));
            Assert.That(errors[1].References.First().Content, Is.EqualTo(variable));
        }

        [Test]
        public void when_verify_columns_and_have_duplicated_columns_in_different_cases_should_return_PL0031_error()
        {
            // arrange
            var variable = "textquestion";
            var variableUpper = "textquestion".ToUpper();
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion(variable: variable)}));

            var preloadedFile = Create.Entity.PreloadedFileInfo(new[] { variable, variableUpper });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { preloadedFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(2));
            Assert.That(errors[0].Code, Is.EqualTo("PL0031"));
            Assert.That(errors[1].Code, Is.EqualTo("PL0031"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(variable));
            Assert.That(errors[1].References.First().Content, Is.EqualTo(variableUpper));
        }

        [Test]
        public void when_verify_columns_and_have_duplicated_columns_in_roster_file_should_return_PL0031_error()
        {
            // arrange
            var variable = "textquestion";
            var roster = "hhroster";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.Roster(variable: roster,
                        children: new[]
                        {
                            Create.Entity.TextQuestion(variable: variable)
                        })
                }));

            var mainFile = Create.Entity.PreloadedFileInfo(new [] {ServiceColumns.InterviewId});
            var rosterFile = Create.Entity.PreloadedFileInfo(
                new[]
                {
                    ServiceColumns.InterviewId, string.Format(ServiceColumns.IdSuffixFormat, roster), variable, variable
                }, fileName: roster, questionnaireOrRosterName: roster);
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { mainFile, rosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(2));
            Assert.That(errors[0].Code, Is.EqualTo("PL0031"));
            Assert.That(errors[1].Code, Is.EqualTo("PL0031"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(variable));
            Assert.That(errors[1].References.First().Content, Is.EqualTo(variable));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(roster));
            Assert.That(errors[1].References.First().DataFile, Is.EqualTo(roster));
        }

        [Test]
        public void when_verify_columns_and_have_gps_altitude_only_should_return_PL0030_error()
        {
            // arrange
            var variable = "gps";
            var gpsAltitudeColumn = $"{variable}__altitude";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.GpsCoordinateQuestion(variable: variable)}));

            var preloadedFile = Create.Entity.PreloadedFileInfo(new[] { gpsAltitudeColumn });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { preloadedFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0030"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(gpsAltitudeColumn));
        }

        [Test]
        public void when_verify_columns_and_roster_file_have_gps_altitude_only_should_return_PL0030_error()
        {
            // arrange
            var variable = "gps";
            var gpsAltitudeColumn = $"{variable}__altitude";
            var roster = "hhroster";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.Roster(variable: roster,
                        children: new[]
                        {
                            Create.Entity.GpsCoordinateQuestion(variable: variable)
                        })
                }));

            var mainFile = Create.Entity.PreloadedFileInfo(new[] { ServiceColumns.InterviewId });
            var rosterFile = Create.Entity.PreloadedFileInfo(
                new[]
                {
                    ServiceColumns.InterviewId, string.Format(ServiceColumns.IdSuffixFormat, roster), gpsAltitudeColumn
                }, fileName: roster, questionnaireOrRosterName: roster);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { mainFile, rosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0030"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(gpsAltitudeColumn));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(roster));
        }

        [Test]
        public void when_verify_columns_and_option_by_multi_question_not_found_should_return_PL0014_error()
        {
            // arrange
            var variable = "categorical";
            var unknownMultiOptionColumn = $"{variable}__4";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.MultyOptionsQuestion(variable: variable, options: Create.Entity.Options(1,2,3))}));

            var preloadedFile = Create.Entity.PreloadedFileInfo(new[] { $"{variable}__1", unknownMultiOptionColumn });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { preloadedFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0014"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(unknownMultiOptionColumn));
        }

        [Test]
        public void when_verify_columns_and_2_options_by_multi_question_not_found_should_return_PL0014_errors()
        {
            // arrange
            var variable = "categorical";
            var unknownMultiOptionColumn1 = $"{variable}__4";
            var unknownMultiOptionColumn2 = $"{variable}__5";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.MultyOptionsQuestion(variable: variable, options: Create.Entity.Options(1,2,3))}));

            var preloadedFile = Create.Entity.PreloadedFileInfo(new[] { $"{variable}__1", unknownMultiOptionColumn1, unknownMultiOptionColumn2 });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { preloadedFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(2));
            Assert.That(errors[0].Code, Is.EqualTo("PL0014"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(unknownMultiOptionColumn1));
            Assert.That(errors[1].Code, Is.EqualTo("PL0014"));
            Assert.That(errors[1].References.First().Content, Is.EqualTo(unknownMultiOptionColumn2));
        }

        [Test]
        public void when_verify_columns_and_option_by_multi_question_with_negative_value_not_found_should_return_PL0014_error()
        {
            // arrange
            var variable = "categorical";
            var unknownMultiOptionColumn = $"{variable}__n4";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.MultyOptionsQuestion(variable: variable, options: Create.Entity.Options(-1))}));

            var preloadedFile = Create.Entity.PreloadedFileInfo(new[] { $"{variable}__n1", unknownMultiOptionColumn });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { preloadedFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0014"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(unknownMultiOptionColumn));
        }

        [Test]
        public void when_verify_columns_in_roster_file_and_2_options_by_multi_question_not_found_should_return_PL0014_errors()
        {
            // arrange
            var variable = "categorical";
            var unknownMultiOptionColumn1 = $"{variable}__4";
            var unknownMultiOptionColumn2 = $"{variable}__n5";

            var roster = "hhroster";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.Roster(variable: roster,
                        children: new[]
                        {
                            Create.Entity.MultyOptionsQuestion(variable: variable, options: Create.Entity.Options(-1,2,3))
                        })
                }));

            var mainFile = Create.Entity.PreloadedFileInfo(new[] { ServiceColumns.InterviewId });
            var rosterFile = Create.Entity.PreloadedFileInfo(
                new[]
                {
                    ServiceColumns.InterviewId, string.Format(ServiceColumns.IdSuffixFormat, roster), $"{variable}__n1", unknownMultiOptionColumn1, unknownMultiOptionColumn2
                }, fileName: roster, questionnaireOrRosterName: roster);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { mainFile, rosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(2));
            Assert.That(errors[0].Code, Is.EqualTo("PL0014"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(unknownMultiOptionColumn1));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(roster));
            Assert.That(errors[1].Code, Is.EqualTo("PL0014"));
            Assert.That(errors[1].References.First().Content, Is.EqualTo(unknownMultiOptionColumn2));
            Assert.That(errors[1].References.First().DataFile, Is.EqualTo(roster));
        }
    }
}
