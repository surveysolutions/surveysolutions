using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;

namespace WB.Tests.Unit.DataExportTests.InterviewErrorsExporterTests
{
    [TestOf(typeof(InterviewErrorsExporter))]
    public class InterviewErrorsExporterTests
    {
        private List<CsvData> dataInCsvFile;
        Guid interviewId = Id.gA;


        [SetUp]
        public void SetUp()
        {
            dataInCsvFile = new List<CsvData>();
        }

        [Test]
        public void should_export_errors_that_are_not_in_roster()
        {
            var questionId = Id.g1;
            var staticTextId = Id.g2;

            var messageForQuestion = "message for question";
            var message1ForStaticText = "message1 for static text";
            var questionnaireDocumentWithOneChapter = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(questionId, variable: "numeric1",
                    validationConditions: new []
                    {
                        new ValidationCondition("1 == 1", messageForQuestion), 
                    }),
                Create.Entity.StaticText(
                    publicKey: staticTextId, 
                    validationConditions: new List<ValidationCondition>
                    {
                        new ValidationCondition("1 == 1", "message for static text"),
                        new ValidationCondition("1 == 1", message1ForStaticText),
                    })
            );

            var questionnaire = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocumentWithOneChapter);

            List<InterviewEntity> exportedEntities = new List<InterviewEntity>();
            exportedEntities.Add(Create.Entity.InterviewEntity(interviewId, EntityType.Question, Create.Identity(questionId), invalidValidations: new []{0}));
            exportedEntities.Add(Create.Entity.InterviewEntity(interviewId, EntityType.StaticText, Create.Identity(staticTextId), invalidValidations: new []{1}));

            var exporter = CreateExporter(questionnaireStorage: questionnaire);

            // Act
            var export = exporter.Export(
                Create.Entity.QuestionnaireExportStructure(questionnaireDocumentWithOneChapter), 
                exportedEntities,
                "",
                "10-20-30-40");

            // Assert
            Assert.That(export, Has.Count.EqualTo(2));
            Assert.That(export[0], Is.EqualTo(new[] { "numeric1", EntityType.Question.ToString(), interviewId.FormatGuid(),"10-20-30-40", 1.ToString(), messageForQuestion }));
            Assert.That(export[1], Is.EqualTo(new[] { "", EntityType.StaticText.ToString(), interviewId.FormatGuid(), "10-20-30-40", 2.ToString(), message1ForStaticText }));
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
            var questionnaireDocumentWithOneChapter = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(questionId, variable: "numeric1",
                    validationConditions: new[]
                    {
                        new ValidationCondition("1 == 1", messageForQuestion),
                    }),
                Create.Entity.NumericIntegerQuestion(disabledQestionId, variable: "numeric11",
                    validationConditions: new[]
                    {
                        new ValidationCondition("1 == 1", messageForQuestion),
                    }),
                Create.Entity.StaticText(
                    publicKey: staticTextId,
                    validationConditions: new List<ValidationCondition>
                    {
                        new ValidationCondition("1 == 1", "message for static text"),
                        new ValidationCondition("1 == 1", message1ForStaticText),
                    }),
                Create.Entity.StaticText(
                    publicKey: disabledStaticTextId,
                    validationConditions: new List<ValidationCondition>
                    {
                        new ValidationCondition("1 == 1", "message for static text"),
                        new ValidationCondition("1 == 1", message1ForStaticText),
                    })
            );

            var questionnaire = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocumentWithOneChapter);

            List<InterviewEntity> exportedEntities = new List<InterviewEntity>();
            exportedEntities.Add(Create.Entity.InterviewEntity(interviewId, EntityType.Question, Create.Identity(questionId), invalidValidations: new[] { 0 }));
            exportedEntities.Add(Create.Entity.InterviewEntity(interviewId, EntityType.StaticText, Create.Identity(staticTextId), invalidValidations: new[] { 1 }));

            exportedEntities.Add(Create.Entity.InterviewEntity(interviewId, EntityType.Question, Create.Identity(disabledQestionId), isEnabled: false, invalidValidations: new[] { 0 }));
            exportedEntities.Add(Create.Entity.InterviewEntity(interviewId, EntityType.StaticText, Create.Identity(disabledStaticTextId), isEnabled:false, invalidValidations: new[] { 1 }));

            var exporter = CreateExporter(questionnaireStorage: questionnaire);

            // Act
            var export = exporter.Export(
                Create.Entity.QuestionnaireExportStructure(questionnaireDocumentWithOneChapter),
                exportedEntities,
                "",
                "10-20-30-40");

            // Assert
            Assert.That(export, Has.Count.EqualTo(2));
            Assert.That(export[0], Is.EqualTo(new[] { "numeric1", EntityType.Question.ToString(), interviewId.FormatGuid(), "10-20-30-40", 1.ToString(), messageForQuestion }));
            Assert.That(export[1], Is.EqualTo(new[] { "", EntityType.StaticText.ToString(), interviewId.FormatGuid(), "10-20-30-40", 2.ToString(), message1ForStaticText }));
        }

        [Test]
        public void should_export_errors_for_entities_within_rosters()
        {
            var questionId = Id.g1;
            var staticTextId = Id.g2;

            var questionMsg = "question msg 1";
            var staticTextInvalid = "static text invalid";
            var questionnaireDocumentWithOneChapter = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(variable: "fixed_roster1", children: new IComposite[]
                {
                    Create.Entity.StaticText(publicKey: staticTextId, 
                        validationConditions: new List<ValidationCondition>
                        {
                            new ValidationCondition("1 == 1", staticTextInvalid)
                        }),
                    Create.Entity.FixedRoster(variable: "fixed_roster_2", 
                        children: new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(id: questionId, variable: "numeric1", 
                                validationConditions: new [] {
                                    new ValidationCondition("1 == 1", "question msg"),
                                    new ValidationCondition("1 == 1", questionMsg),
                                })
                        })
                })
            );

            List<InterviewEntity> exportedEntities = new List<InterviewEntity>();
            exportedEntities.Add(Create.Entity.InterviewEntity(interviewId, EntityType.StaticText, Create.Identity(staticTextId, 0), invalidValidations: new[] { 0 }));
            exportedEntities.Add(Create.Entity.InterviewEntity(interviewId, EntityType.Question, Create.Identity(questionId, 0, 1), invalidValidations: new[] { 1 }));

            var questionnaire = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocumentWithOneChapter);
            var exporter = CreateExporter(questionnaireStorage: questionnaire);

            // Act
            var export = exporter.Export(Create.Entity.QuestionnaireExportStructure(questionnaireDocumentWithOneChapter),
                exportedEntities,
                "",
                "10-20-30-40");

            // Assert
            Assert.That(export, Has.Count.EqualTo(2));
            Assert.That(export[0], Is.EqualTo(new[] { "", EntityType.StaticText.ToString(), "fixed_roster1", interviewId.FormatGuid(), "10-20-30-40", "0", "", "1", staticTextInvalid }));
            Assert.That(export[1], Is.EqualTo(new[] { "numeric1", EntityType.Question.ToString(), "fixed_roster_2", interviewId.FormatGuid(), "10-20-30-40", "0", "1", "2", questionMsg }));
        }

        private InterviewErrorsExporter CreateExporter(IInterviewFactory interviewFactory = null, 
            ICsvWriter csvWriter = null, 
            IQuestionnaireStorage questionnaireStorage = null)
        {
            return new InterviewErrorsExporter(
                csvWriter ??  Create.Service.CsvWriter(dataInCsvFile), 
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IFileSystemAccessor>());
        }
    }
}
