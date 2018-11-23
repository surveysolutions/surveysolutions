using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    [TestOf(typeof(InterviewErrorsExporter))]
    public class InterviewErrorsExporterTests
    {
        private List<Create.CsvData> dataInCsvFile;
        private readonly Guid interviewId = Id.gA;

        [SetUp]
        public void SetUp()
        {
            dataInCsvFile = new List<Create.CsvData>();
        }

        [Test]
        public void should_export_errors_that_are_not_in_roster()
        {
            var questionId = Id.g1;
            var staticTextId = Id.g2;

            var messageForQuestion = "message for question";
            var message1ForStaticText = "message1 for static text";
            var questionnaireDocumentWithOneChapter = Create.QuestionnaireDocument(
                Id.g1,
                "variable",
                Create.NumericIntegerQuestion(questionId, "numeric1",
                    validationConditions: new[]
                    {
                        new ValidationCondition("1 == 1", messageForQuestion)
                    }),
                Create.StaticText(
                    staticTextId,
                    new List<ValidationCondition>
                    {
                        new ValidationCondition("1 == 1", "message for static text"),
                        new ValidationCondition("1 == 1", message1ForStaticText)
                    })
            );

            var exportedEntities = new List<InterviewEntity>();
            exportedEntities.Add(Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(questionId),
                new[] {0}));
            exportedEntities.Add(Create.InterviewEntity(interviewId, EntityType.StaticText,
                Create.Identity(staticTextId), new[] {1}));

            var exporter = CreateExporter();

            // Act
            var export = exporter.Export(
                Create.QuestionnaireExportStructure(questionnaireDocumentWithOneChapter),
                questionnaireDocumentWithOneChapter,
                exportedEntities,
                "",
                "10-20-30-40");

            // Assert
            Assert.That(export, Has.Count.EqualTo(2));
            Assert.That(export[0],
                Is.EqualTo(new[]
                {
                    "10-20-30-40", interviewId.FormatGuid(), "numeric1", EntityType.Question.ToString(), 1.ToString(),
                    messageForQuestion
                }));
            Assert.That(export[1],
                Is.EqualTo(new[]
                {
                    "10-20-30-40", interviewId.FormatGuid(), "", EntityType.StaticText.ToString(), 2.ToString(),
                    message1ForStaticText
                }));
        }

        [Test]
        public void should_export_errors_and_entities_are_disabled()
        {
            var questionId = Id.g1;
            var staticTextId = Id.g2;

            var disabledQestionId = Id.g9;
            var disabledStaticTextId = Id.g10;

            var messageForQuestion = "message for question";
            var message1ForStaticText = "message1 for static text";
            var questionnaireDocumentWithOneChapter = Create.QuestionnaireDocumentWithOneChapter(
                children: new IQuestionnaireEntity[]
                {
                    Create.NumericIntegerQuestion(questionId, "numeric1",
                        validationConditions: new[]
                        {
                            new ValidationCondition("1 == 1", messageForQuestion)
                        }),
                    Create.NumericIntegerQuestion(disabledQestionId, "numeric11",
                        validationConditions: new[]
                        {
                            new ValidationCondition("1 == 1", messageForQuestion)
                        }),
                    Create.StaticText(
                        staticTextId,
                        new List<ValidationCondition>
                        {
                            new ValidationCondition("1 == 1", "message for static text"),
                            new ValidationCondition("1 == 1", message1ForStaticText)
                        }),
                    Create.StaticText(
                        disabledStaticTextId,
                        new List<ValidationCondition>
                        {
                            new ValidationCondition("1 == 1", "message for static text"),
                            new ValidationCondition("1 == 1", message1ForStaticText)
                        })
                }
            );
            questionnaireDocumentWithOneChapter.ConnectChildrenWithParent();

            var exportedEntities = new List<InterviewEntity>();
            exportedEntities.Add(Create.InterviewEntity(interviewId, EntityType.Question, Create.Identity(questionId),
                new[] {0}));
            exportedEntities.Add(Create.InterviewEntity(interviewId, EntityType.StaticText,
                Create.Identity(staticTextId), new[] {1}));

            exportedEntities.Add(Create.InterviewEntity(interviewId, EntityType.Question,
                Create.Identity(disabledQestionId), isEnabled: false, invalidValidations: new[] {0}));
            exportedEntities.Add(Create.InterviewEntity(interviewId, EntityType.StaticText,
                Create.Identity(disabledStaticTextId), isEnabled: false, invalidValidations: new[] {1}));

            var exporter = CreateExporter();

            // Act
            var export = exporter.Export(
                Create.QuestionnaireExportStructure(questionnaireDocumentWithOneChapter),
                questionnaireDocumentWithOneChapter,
                exportedEntities,
                "",
                "10-20-30-40");

            // Assert
            Assert.That(export, Has.Count.EqualTo(2));
            Assert.That(export[0],
                Is.EqualTo(new[]
                {
                    "10-20-30-40", interviewId.FormatGuid(), "numeric1", EntityType.Question.ToString(), 1.ToString(),
                    messageForQuestion
                }));
            Assert.That(export[1],
                Is.EqualTo(new[]
                {
                    "10-20-30-40", interviewId.FormatGuid(), "", EntityType.StaticText.ToString(), 2.ToString(),
                    message1ForStaticText
                }));
        }

        [Test]
        public void should_export_errors_for_entities_within_rosters()
        {
            var questionId = Id.g1;
            var staticTextId = Id.g2;

            var questionMsg = "question msg 1";
            var staticTextInvalid = "static text invalid";
            var questionnaireDocumentWithOneChapter = Create.QuestionnaireDocumentWithOneChapter(
                children: new IQuestionnaireEntity[]
                {
                    Create.Roster(variable: "fixed_roster1", children: new IQuestionnaireEntity[]
                    {
                        Create.StaticText(staticTextId,
                            new List<ValidationCondition>
                            {
                                new ValidationCondition("1 == 1", staticTextInvalid)
                            }),
                        Create.Roster(variable: "fixed_roster_2",
                            children: new IQuestionnaireEntity[]
                            {
                                Create.NumericIntegerQuestion(questionId, "numeric1",
                                    validationConditions: new[]
                                    {
                                        new ValidationCondition("1 == 1", "question msg"),
                                        new ValidationCondition("1 == 1", questionMsg)
                                    })
                            })
                    })
                }
            );
            questionnaireDocumentWithOneChapter.ConnectChildrenWithParent();

            var exportedEntities = new List<InterviewEntity>();
            exportedEntities.Add(Create.InterviewEntity(interviewId, EntityType.StaticText,
                Create.Identity(staticTextId, 0), new[] {0}));
            exportedEntities.Add(Create.InterviewEntity(interviewId, EntityType.Question,
                Create.Identity(questionId, 0, 1), new[] {1}));

            var exporter = CreateExporter();

            // Act
            var export = exporter.Export(Create.QuestionnaireExportStructure(questionnaireDocumentWithOneChapter),
                questionnaireDocumentWithOneChapter,
                exportedEntities,
                "",
                "10-20-30-40");

            // Assert
            Assert.That(export, Has.Count.EqualTo(2));
            Assert.That(export[0],
                Is.EqualTo(new[]
                {
                    "10-20-30-40", interviewId.FormatGuid(), "fixed_roster1", "0", "", 
                    "", EntityType.StaticText.ToString(), "1", staticTextInvalid
                }));
            Assert.That(export[1],
                Is.EqualTo(new[]
                {
                    "10-20-30-40", interviewId.FormatGuid(), "fixed_roster_2", "0", "1",
                    "numeric1", EntityType.Question.ToString(), "2", questionMsg
                }));
        }

        private InterviewErrorsExporter CreateExporter(ICsvWriter csvWriter = null)
        {
            return new InterviewErrorsExporter(
                csvWriter ?? Create.CsvWriter(dataInCsvFile),
                Mock.Of<IFileSystemAccessor>());
        }
    }
}
