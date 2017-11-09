using System;
using System.Collections.Generic;
using System.Threading;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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

            var questionError = new ExportedError
            {
                InterviewId = interviewId,
                EntityId = questionId,
                RosterVector = Create.RosterVector(),
                EntityType = EntityType.Question,
                FailedValidationConditions = new[] {0}
            };
            var staticTextError = new ExportedError
            {
                InterviewId = interviewId,
                EntityId = staticTextId,
                RosterVector = Create.RosterVector(),
                EntityType = EntityType.StaticText,
                FailedValidationConditions = new[] { 1 }
            };

            var interviewFactory = Mock.Of<IInterviewFactory>(x =>
                x.GetErrors(It.IsAny<IEnumerable<Guid>>()) == new List<ExportedError> {questionError, staticTextError});
            var exporter = CreateExporter(interviewFactory, questionnaireStorage: questionnaire);

            // Act
            exporter.Export(Create.Entity.QuestionnaireExportStructure(questionnaireDocumentWithOneChapter), 
                new List<Guid>{interviewId}, 
                "blah", 
                new Progress<int>(), 
                CancellationToken.None);

            // Assert
            Assert.That(dataInCsvFile[0].Data[0], Is.EqualTo(new[] { "Variable", "Type", "InterviewId", "Message Number", "Message" }));
            Assert.That(dataInCsvFile[1].Data[0], Is.EqualTo(new[] { "numeric1", EntityType.Question.ToString(), interviewId.FormatGuid(), 1.ToString(), messageForQuestion }));
            Assert.That(dataInCsvFile[1].Data[1], Is.EqualTo(new[] { "", EntityType.StaticText.ToString(), interviewId.FormatGuid(), 2.ToString(), message1ForStaticText }));
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

            var staticTextError = new ExportedError
            {
                InterviewId = interviewId,
                EntityId = staticTextId,
                RosterVector = Create.RosterVector(0),
                EntityType = EntityType.StaticText,
                FailedValidationConditions = new[] { 0 }
            };
            var questionError = new ExportedError
            {
                InterviewId = interviewId,
                EntityId = questionId,
                RosterVector = Create.RosterVector(0, 1),
                EntityType = EntityType.Question,
                FailedValidationConditions = new[] { 1 }
            };

            var interviewFactory = Mock.Of<IInterviewFactory>(x =>
                x.GetErrors(It.IsAny<IEnumerable<Guid>>()) == new List<ExportedError> {questionError, staticTextError});
            var questionnaire = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocumentWithOneChapter);
            var exporter = CreateExporter(interviewFactory, questionnaireStorage: questionnaire);

            // Act
            exporter.Export(Create.Entity.QuestionnaireExportStructure(questionnaireDocumentWithOneChapter),
                new List<Guid> { interviewId },
                "blah",
                new Progress<int>(),
                CancellationToken.None);

            // Assert
            Assert.That(dataInCsvFile[0].Data[0], Is.EqualTo(new[] { "Variable", "Type", "Roster", "InterviewId", "Id1", "Id2", "Message Number", "Message" }));
            Assert.That(dataInCsvFile[1].Data[0], Is.EqualTo(new[] { "numeric1", EntityType.Question.ToString(), "fixed_roster_2", interviewId.FormatGuid(), "0", "1", "2", questionMsg }));
            Assert.That(dataInCsvFile[1].Data[1], Is.EqualTo(new[] { "", EntityType.StaticText.ToString(), "fixed_roster1", interviewId.FormatGuid(), "0", "", "1", staticTextInvalid }));
        }

        private InterviewErrorsExporter CreateExporter(IInterviewFactory interviewFactory = null, 
            ICsvWriter csvWriter = null, 
            IQuestionnaireStorage questionnaireStorage = null)
        {
            return new InterviewErrorsExporter(
                interviewFactory ?? Mock.Of<IInterviewFactory>(), 
                Mock.Of<ILogger>(), 
                csvWriter ??  Create.Service.CsvWriter(dataInCsvFile), 
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                Create.Service.TransactionManagerProvider());
        }
    }
}