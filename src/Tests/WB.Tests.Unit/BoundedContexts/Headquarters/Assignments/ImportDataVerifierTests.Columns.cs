using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
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

        [Test]
        public void when_verify_columns_and_archive_have_roster_files_and_main_file_without_interviewid_column_should_return_PL0007_error()
        {
            // arrange
            var roster = "myroster";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.FixedRoster(variable: roster,
                        children: new[]
                        {
                            Create.Entity.TextQuestion()
                        })
                }));

            var mainFile = Create.Entity.PreloadedFileInfo(new string[0]);
            var rosterFile = Create.Entity.PreloadedFileInfo(
                new[]
                {
                    ServiceColumns.InterviewId, string.Format(ServiceColumns.IdSuffixFormat, roster)
                }, fileName: roster, questionnaireOrRosterName: roster);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { mainFile, rosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0007"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(ServiceColumns.InterviewId));
        }

        [Test]
        public void when_verify_columns_and_archive_have_roster_file_without_interviewid_column_should_return_PL0007_error()
        {
            // arrange
            var roster = "myroster";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.FixedRoster(variable: roster,
                        children: new[]
                        {
                            Create.Entity.TextQuestion()
                        })
                }));

            var mainFile = Create.Entity.PreloadedFileInfo(new[] {ServiceColumns.InterviewId});
            var rosterFile = Create.Entity.PreloadedFileInfo(
                new[]
                {
                    string.Format(ServiceColumns.IdSuffixFormat, roster)
                }, fileName: roster, questionnaireOrRosterName: roster);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { mainFile, rosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0007"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(roster));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(ServiceColumns.InterviewId));
        }

        [Test]
        public void when_verify_columns_and_roster_file_without_roster_instance_id_column_should_return_PL0007_error()
        {
            // arrange
            var roster = "myroster";
            Guid rosterSizeId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(rosterSizeId), 
                    Create.Entity.Roster(variable: roster, rosterSizeQuestionId: rosterSizeId,
                    children: new[]
                    {
                        Create.Entity.TextQuestion()
                    })));

            var mainFile = Create.Entity.PreloadedFileInfo(new[] { ServiceColumns.InterviewId });
            var rosterFile = Create.Entity.PreloadedFileInfo(
                new[]
                {
                    ServiceColumns.InterviewId
                }, fileName: roster, questionnaireOrRosterName: roster);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { mainFile, rosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0007"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(roster));
            Assert.That(errors[0].References.First().Content, Is.EqualTo($"{roster}__id"));
        }

        [Test]
        public void when_verify_columns_and_nested_roster_file_without_parent_roster_instance_column_should_return_PL0007_error()
        {
            // arrange
            var roster = "myroster";
            var nestedRoster = "nestedroster";
            Guid rosterSizeId = Guid.Parse("11111111111111111111111111111111");
            Guid nestedRosterSizeId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(rosterSizeId),
                    Create.Entity.Roster(variable: roster, rosterSizeQuestionId: rosterSizeId,
                        children: new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(nestedRosterSizeId),
                            Create.Entity.Roster(variable: nestedRoster, rosterSizeQuestionId: nestedRosterSizeId,
                                children: new[]
                                {
                                    Create.Entity.TextQuestion()
                                })
                        })));

            var mainFile = Create.Entity.PreloadedFileInfo(new[] { ServiceColumns.InterviewId });
            var rosterFile = Create.Entity.PreloadedFileInfo(
                new[]
                {
                    ServiceColumns.InterviewId, string.Format(ServiceColumns.IdSuffixFormat, roster)
                }, fileName: roster, questionnaireOrRosterName: roster);
            var nestedRosterFile = Create.Entity.PreloadedFileInfo(
                new[]
                {
                    ServiceColumns.InterviewId, string.Format(ServiceColumns.IdSuffixFormat, nestedRoster)
                }, fileName: nestedRoster, questionnaireOrRosterName: nestedRoster);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { mainFile, rosterFile, nestedRosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0007"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(nestedRoster));
            Assert.That(errors[0].References.First().Content, Is.EqualTo($"{roster}__id"));
        }

        [Test]
        public void when_verify_columns_and_fixed_roster_file_without_roster_instance_id_column_should_return_PL0007_error()
        {
            // arrange
            var roster = "myroster";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.Roster(variable: roster, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: Create.Entity.FixedTitles(10, 20),
                        children: new[]
                        {
                            Create.Entity.TextQuestion()
                        })));

            var mainFile = Create.Entity.PreloadedFileInfo(new[] { ServiceColumns.InterviewId });
            var rosterFile = Create.Entity.PreloadedFileInfo(
                new[]
                {
                    ServiceColumns.InterviewId
                }, fileName: roster, questionnaireOrRosterName: roster);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { mainFile, rosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0007"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(roster));
            Assert.That(errors[0].References.First().Content, Is.EqualTo($"{roster}__id"));
        }

        //[Test]
        //public void when_verify_columns_in_zip_file_with_roster_and_nested_roster_files_which_triggered_by_1_roster_size_question_and_nested_roster_file_dont_have_parent_roster_id_column_should_not_return_empty_errors()
        //{
        //    // arrange
        //    var roster = "myroster";
        //    var nestedRoster = "nestedroster";
        //    Guid rosterSizeId = Guid.Parse("11111111111111111111111111111111");
        //    var questionnaire = Create.Entity.PlainQuestionnaire(
        //        Create.Entity.QuestionnaireDocumentWithOneChapter(
        //            Create.Entity.NumericIntegerQuestion(rosterSizeId),
        //            Create.Entity.Roster(variable: roster, rosterSizeQuestionId: rosterSizeId,
        //                children: new IComposite[]
        //                {
        //                    Create.Entity.Roster(variable: nestedRoster, rosterSizeQuestionId: rosterSizeId,
        //                        children: new[]
        //                        {
        //                            Create.Entity.TextQuestion()
        //                        })
        //                })));

        //    var mainFile = Create.Entity.PreloadedFileInfo(new[] { ServiceColumns.InterviewId });
        //    var rosterFile = Create.Entity.PreloadedFileInfo(
        //        new[]
        //        {
        //            ServiceColumns.InterviewId, string.Format(ServiceColumns.IdSuffixFormat, roster)
        //        }, fileName: roster, questionnaireOrRosterName: roster);
        //    var nestedRosterFile = Create.Entity.PreloadedFileInfo(
        //        new[]
        //        {
        //            ServiceColumns.InterviewId, string.Format(ServiceColumns.IdSuffixFormat, nestedRoster)
        //        }, fileName: nestedRoster, questionnaireOrRosterName: nestedRoster);

        //    var verifier = Create.Service.ImportDataVerifier();

        //    // act
        //    var errors = verifier.VerifyColumns(new[] { mainFile, rosterFile, nestedRosterFile }, questionnaire).ToArray();

        //    // assert
        //    Assert.That(errors, Is.Empty);
        //}

        [Test]
        public void when_verify_columns_and_roster_file_has_roster_instance_id_column_in_old_format_should_return_empty_errors()
        {
            // arrange
            var roster = "myroster";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.Roster(variable: roster, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: Create.Entity.FixedTitles(10, 20),
                        children: new[]
                        {
                            Create.Entity.TextQuestion()
                        })));

            var mainFile = Create.Entity.PreloadedFileInfo(new[] { ServiceColumns.InterviewId });
            var rosterFile = Create.Entity.PreloadedFileInfo(
                new[]
                {
                    ServiceColumns.InterviewId, "ParentId1"
                }, fileName: roster, questionnaireOrRosterName: roster);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] { mainFile, rosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        //[Test]
        //public void when_verify_columns_and_nested_roster_file_has_roster_instance_id_columns_in_new_and_old_format_should_return_empty_errors()
        //{
        //    // arrange
        //    var roster = "myroster";
        //    var nestedRoster = "nestedroster";
        //    Guid rosterSizeId = Guid.Parse("11111111111111111111111111111111");
        //    var questionnaire = Create.Entity.PlainQuestionnaire(
        //        Create.Entity.QuestionnaireDocumentWithOneChapter(
        //            Create.Entity.NumericIntegerQuestion(rosterSizeId),
        //            Create.Entity.Roster(variable: roster, rosterSizeQuestionId: rosterSizeId,
        //                children: new IComposite[]
        //                {
        //                    Create.Entity.Roster(variable: nestedRoster, rosterSizeQuestionId: rosterSizeId,
        //                        children: new[]
        //                        {
        //                            Create.Entity.TextQuestion()
        //                        })
        //                })));

        //    var mainFile = Create.Entity.PreloadedFileInfo(new[] { ServiceColumns.InterviewId });
        //    var nestedRosterFile = Create.Entity.PreloadedFileInfo(
        //        new[]
        //        {
        //            ServiceColumns.InterviewId, "ParentId1", string.Format(ServiceColumns.IdSuffixFormat, nestedRoster)
        //        }, fileName: nestedRoster, questionnaireOrRosterName: nestedRoster);

        //    var verifier = Create.Service.ImportDataVerifier();

        //    // act
        //    var errors = verifier.VerifyColumns(new[] { mainFile, nestedRosterFile }, questionnaire).ToArray();

        //    // assert
        //    Assert.That(errors, Is.Empty);
        //}

        [Test]
        public void when_verify_columns_and_1_column_is_empty_should_return_PL0003_error()
        {
            // arrange
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion()));

            var mainFile = Create.Entity.PreloadedFileInfo(new[] {""});

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyColumns(new[] {mainFile}, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0003"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(""));
        }
    }
}
