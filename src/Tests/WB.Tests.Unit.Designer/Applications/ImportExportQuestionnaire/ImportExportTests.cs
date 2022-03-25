using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;
using WB.UI.Designer.Code.ImportExport;
using Questionnaire = WB.Core.BoundedContexts.Designer.ImportExport.Models.Questionnaire;

namespace WB.Tests.Unit.Designer.Applications.ImportExportQuestionnaire
{
    [TestOf(typeof(ImportExportQuestionnaireMapper))]
    public class ImportExportTests
    {
        [Test]
        public void when_export_empty_questionnaire_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1, children: new[]
            {
                Create.Group()
            });
            questionnaireDocument.Title = "test";

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);

            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            errors.Count.Should().Be(0);
        }   
        
        [Test]
        public void when_export_questionnaire_with_info_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1);
            questionnaireDocument.Title = "test";
            questionnaireDocument.Description = "desc";
            var documentPublicKey = Guid.NewGuid();
            questionnaireDocument.Id = documentPublicKey.FormatGuid();
            questionnaireDocument.PublicKey = documentPublicKey;
            //questionnaireDocument.Revision = 17;
            //questionnaireDocument.CloseDate = DateTime.Now;
            //questionnaireDocument.ConditionExpression = "ConditionExpression";
            questionnaireDocument.CreatedBy = Guid.NewGuid();
            questionnaireDocument.CreationDate = DateTime.Now;
            //questionnaireDocument.DefaultTranslation = Guid.NewGuid();
            //questionnaireDocument.IsDeleted = true;
            //questionnaireDocument.IsPublic = true;
            //questionnaireDocument.OpenDate = DateTime.Now;
            questionnaireDocument.VariableName = "VariableName";
            //questionnaireDocument.DefaultLanguageName = "DefaultLanguageName";
            questionnaireDocument.HideIfDisabled = true;
            questionnaireDocument.LastEntryDate = DateTime.Now;
            //questionnaireDocument.LastEventSequence = 777;
            questionnaireDocument.Metadata = new QuestionnaireMetaInfo()
            {
                Consultant = "Consultant",
                Country = "Country",
                Coverage = "Coverage",
                Funding = "Funding",
                Keywords = "Keywords",
                Language = "Language",
                Notes = "Notes",
                Universe = "Universe",
                Version = "Version",
                Year = 111,
                PrimaryInvestigator = "PrimaryInvestigator",
                StudyType = StudyType.EnterpriseSurvey,
                SubTitle = "SubTitle",
                VersionNotes = "VersionNotes",
                KindOfData = "KindOfData",
                UnitOfAnalysis = "UnitOfAnalysis",
                ModeOfDataCollection = ModeOfDataCollection.Mail,
                AgreeToMakeThisQuestionnairePublic = true,
            };
            var coverPageSectionId = Guid.NewGuid();
            questionnaireDocument.CoverPageSectionId = coverPageSectionId;
            questionnaireDocument.Children = new List<IComposite>()
            {
                new Group() { PublicKey = coverPageSectionId, ConditionExpression = null, VariableName = "cover" },
                new Group() { ConditionExpression = "true", VariableName = "chapter" }
            }.ToReadOnlyCollection();

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);

            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_questionnaire_with_cover_page_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1, title: "Test");
            var coverPageSectionId = Id.g7;
            questionnaireDocument.CoverPageSectionId = coverPageSectionId;
            questionnaireDocument.Children = new List<IComposite>()
            {
                new Group() { PublicKey = coverPageSectionId, ConditionExpression = null, VariableName = "cover"},
                new Group() { ConditionExpression = "true", VariableName = "chapter" },
            }.ToReadOnlyCollection();

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);

            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            errors.Count.Should().Be(0);
        }
       
        [Test]
        public void when_export_empty_chapter_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter()
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_one_chapter_should_be_equals_after_import()
        {
            var chapter = Create.Chapter(
                title: " Chapter",
                chapterId: Guid.NewGuid(),
                hideIfDisabled: true
            );
            chapter.Enabled = true;
            chapter.ConditionExpression = "ConditionExpression";
            chapter.IsRoster = false;
            chapter.VariableName = "VariableName";
            chapter.HideIfDisabled = true;

            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                chapter
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);

            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_one_chapter_with_roster_should_be_equals_after_import()
        {
            var roster = Create.Roster(title: "Roster");
            roster.Enabled = true;
            roster.ConditionExpression = "ConditionExpression";
            roster.DisplayMode = RosterDisplayMode.Table;
            roster.IsRoster = true;
            roster.VariableName = "VariableName";
            roster.CustomRosterTitle = false;
            roster.HideIfDisabled = true;
            roster.FixedRosterTitles = new FixedRosterTitle[]
            {
                new FixedRosterTitle(11, "rTitle11"),
                new FixedRosterTitle(22, "rTitle22"),
            };
            roster.RosterSizeSource = RosterSizeSourceType.FixedTitles;
            roster.RosterSizeQuestionId = Guid.NewGuid();
            roster.RosterTitleQuestionId = Guid.NewGuid();
            
            var chapter = Create.Chapter(
                title: " Chapter",
                chapterId: Guid.NewGuid(),
                hideIfDisabled: true,
                children: new IComposite[]
                {
                    roster,
                    Create.NumericIntegerQuestion(id: roster.RosterSizeQuestionId, variable: "num-trigger", enablementCondition: "enable"),
                    Create.TextQuestion(questionId: roster.RosterTitleQuestionId, variable: "title-question", enablementCondition: "enable"),
                }
            );
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                chapter
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);

            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            ((Group)newQuestionnaire.Children[1].Children[0]).RosterSizeQuestionId.Should()
                .Be(newQuestionnaire.Children[1].Children[1].PublicKey);
            ((Group)newQuestionnaire.Children[1].Children[0]).RosterTitleQuestionId.Should()
                .Be(newQuestionnaire.Children[1].Children[2].PublicKey);
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_questionnaire_with_attachment_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter()
            );
            questionnaireDocument.Attachments = new List<Attachment>()
            {
                new Attachment() { Name = "attach #1", AttachmentId = Guid.NewGuid()/*, ContentId = "content1"*/},
                new Attachment() { Name = "attach #2", AttachmentId = Guid.NewGuid()/*, ContentId = "content2"*/},
            };

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors, q =>
                {
                    q.Attachments[0].FileName = "attachment1.jpeg";
                    q.Attachments[0].ContentType = "image/jpeg";
                    q.Attachments[1].FileName = "attachment2.png";
                    q.Attachments[1].ContentType = "image/png";
                });
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_questionnaire_with_categories_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter()
            );
            questionnaireDocument.Categories = new List<Categories>()
            {
                new Categories() { Name = "Categories #1", Id = Guid.NewGuid() },
                new Categories() { Name = "Categories #2", Id = Guid.NewGuid() },
            };

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors,
                q =>
                {
                    q.Categories[0].FileName = "categories1.json";
                    q.Categories[1].FileName = "categories2.json";
                });
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_questionnaire_with_macroses_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter()
            );
            questionnaireDocument.Macros = new Dictionary<Guid, Macro>()
            {
                { Guid.NewGuid(), new Macro() { Name = "Macro #1", Description = "Description 1", Content = "content1"} },
                { Guid.NewGuid(), new Macro() { Name = "Macro #2", Description = "Description 2", Content = "content2"} },
            };

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Macros.Values.Should().BeEquivalentTo(newQuestionnaire.Macros.Values);
            newQuestionnaire.Macros.Values.Should().BeEquivalentTo(questionnaireDocument.Macros.Values);
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_questionnaire_with_translations_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter()
            );
            questionnaireDocument.Translations = new List<Translation>()
            {
                new Translation() { Name = "Translation #1", Id = Guid.NewGuid() },
                new Translation() { Name = "Translation #2", Id = Guid.NewGuid() },
            };
            questionnaireDocument.DefaultLanguageName = "Custom name";
            //questionnaireDocument.DefaultTranslation = questionnaireDocument.Translations.First().Id;

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions(mi => mi.Name == nameof(Translation.Id)));
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions(mi => mi.Name == nameof(Translation.Id)));
            errors.Count.Should().Be(2);
            errors.First().Should().Be("Required properties are missing from object: FileName.");
            errors.Second().Should().Be("Required properties are missing from object: FileName.");
        }
        
        [Test]
        public void when_export_questionnaire_with_lookup_tables_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter()
            );
            questionnaireDocument.LookupTables = new Dictionary<Guid, LookupTable>()
            {
                { Guid.NewGuid(), new LookupTable() { FileName = "FileName #1", TableName = "TableName 1", } },
                { Guid.NewGuid(), new LookupTable() { FileName = "FileName #2", TableName = "TableName 2", } },
            };

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.LookupTables.Values.Should().BeEquivalentTo(newQuestionnaire.LookupTables.Values);
            newQuestionnaire.LookupTables.Values.Should().BeEquivalentTo(questionnaireDocument.LookupTables.Values);
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_empty_text_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                 Create.Chapter(
                     children: new []{Create.TextQuestion(enablementCondition: "enable") }
                 )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_text_question_should_be_equals_after_import()
        {
            var textQuestion = Create.TextQuestion(
                questionId: Guid.NewGuid(),
                text: "Title",
                enablementCondition: "enablementCondition",
                //validationExpression: "validationExpression",
                mask: "mask",
                variable: "variable",
                //validationMessage: "validationMessage",
                scope: QuestionScope.Supervisor,
                //preFilled: true,
                label: "label",
                instruction: "instruction",
                validationConditions: new ValidationCondition[] { Create.ValidationCondition() },
                hideIfDisabled: true
                );
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { textQuestion }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_empty_numeric_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                 Create.Chapter(
                     children: new []
                     {
                         Create.NumericIntegerQuestion(variable: "int", enablementCondition: "enable"), 
                         Create.NumericRealQuestion(variable: "real", enablementCondition: "enable")
                     }
                 )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_numeric_question_should_be_equals_after_import()
        {
            var numericQuestion = Create.NumericIntegerQuestion(
                id: Guid.NewGuid(), 
                variable: "variable", 
                enablementCondition: "enablementCondition",
                //validationExpression: "validationExpression", 
                scope: QuestionScope.Supervisor,
                isPrefilled: false,
                hideIfDisabled: true,
                validationConditions: new []{ Create.ValidationCondition() }, 
                //linkedToRosterId: Guid.NewGuid(),
                title: "numeric", 
                variableLabel: "label", 
                options: new []{ new Option("1", "option1") }
                );
            numericQuestion.CountOfDecimalPlaces = 7;
            numericQuestion.UseFormatting = true;
            numericQuestion.Properties = new QuestionProperties(hideInstructions: true, useFormatting: true);
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { numericQuestion }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }



        [Test]
        public void when_export_empty_audio_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new AudioQuestion(), 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_audio_question_should_be_equals_after_import()
        {
            var question = new AudioQuestion()
            {
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                StataExportCaption = "stata caption",
                HideIfDisabled = true,
            };
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { question }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_empty_area_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new AreaQuestion(), 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_area_question_should_be_equals_after_import()
        {
            var question = new AreaQuestion()
            {
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                StataExportCaption = "stata caption",
                HideIfDisabled = true,
                Properties = new QuestionProperties(hideInstructions: true, useFormatting: false)
                {
                    GeometryType = GeometryType.Polygon,
                }
            };
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { question }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_empty_datetime_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new DateTimeQuestion(), 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
           
        [Test]
        public void when_export_datetime_question_should_be_equals_after_import()
        {
            var question = new DateTimeQuestion()
            {
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                StataExportCaption = "stata caption",
                HideIfDisabled = true,
                IsTimestamp = true,
                Properties = new QuestionProperties(hideInstructions: true, useFormatting: false)
                {
                    DefaultDate = DateTime.Now
                }
            };
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { question }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_empty_gps_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new GpsCoordinateQuestion(), 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
                   
        [Test]
        public void when_export_gps_question_should_be_equals_after_import()
        {
            var question = new GpsCoordinateQuestion()
            {
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                StataExportCaption = "stata caption",
                HideIfDisabled = true,
            };
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { question }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_empty_multimedia_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new MultimediaQuestion(), 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
                   
        [Test]
        public void when_export_multimedia_question_should_be_equals_after_import()
        {
            var question = new MultimediaQuestion()
            {
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                StataExportCaption = "stata caption",
                HideIfDisabled = true,
                IsSignature = true
            };
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { question }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_empty_multi_options_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new MultyOptionsQuestion(), 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
                   
        [Test]
        public void when_export_multioptions_question_should_be_equals_after_import()
        {
            var question = new MultyOptionsQuestion()
            {
                PublicKey = Id.g7,
                QuestionText = "question title",
                VariableLabel = "variable",
                StataExportCaption = "variable-name",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                HideIfDisabled = true,
                AreAnswersOrdered = true,
                MaxAllowedAnswers = 7,
                YesNoView = true,
                CategoriesId = Guid.NewGuid(),
                Answers = new List<Answer>()
                {
                    new Answer(),
                    new Answer(),
                    new Answer() { AnswerText = "text1", AnswerValue = "111", },
                    new Answer() { AnswerText = "text2", AnswerValue = "222", ParentValue = "111"},
                },
                //LinkedToRosterId = Guid.NewGuid(),
                LinkedToQuestionId = Id.g7,
                LinkedFilterExpression = "filter expression",
                // Properties = new QuestionProperties(hideInstructions: true, useFormatting: false)
                // {
                //     OptionsFilterExpression = "OptionsFilterExpression"
                // }
            };
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { question }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_empty_qr_barcode_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new QRBarcodeQuestion(), 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
                   
        [Test]
        public void when_export_qr_barcode_question_should_be_equals_after_import()
        {
            var question = new QRBarcodeQuestion()
            {
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                StataExportCaption = "stata caption",
                HideIfDisabled = true,
            };
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { question }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_empty_single_option_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new SingleQuestion(), 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
                   
        [Test]
        public void when_export_single_option_question_should_be_equals_after_import()
        {
            var question = new SingleQuestion()
            {
                PublicKey = Id.g7,
                QuestionText = "question title",
                VariableLabel = "variable",
                StataExportCaption = "varName",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                HideIfDisabled = true,
                ShowAsList = true,
                ShowAsListThreshold = 77,
                CategoriesId = Guid.NewGuid(),
                Answers = new List<Answer>()
                {
                    new Answer() { AnswerText = "text1", AnswerValue = "111", },
                    new Answer() { AnswerText = "text2", AnswerValue = "222", ParentValue = "111"},
                },
                LinkedToQuestionId = Id.g7,
                LinkedFilterExpression = "filter expression",
                CascadeFromQuestionId = Guid.NewGuid(),
                //IsFilteredCombobox = true,
            };
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        question,
                        Create.SingleQuestion(id: question.CascadeFromQuestionId, variable: "cascade-parent", isFilteredCombobox: true, enablementCondition: "enable"),
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_empty_text_list_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new TextListQuestion(), 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
                   
        [Test]
        public void when_export_text_list_question_should_be_equals_after_import()
        {
            var question = new TextListQuestion()
            {
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                StataExportCaption = "stata caption",
                HideIfDisabled = true,
                MaxAnswerCount = 77,
            };
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { question }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_empty_variable_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        Create.Variable(), 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
                   
        [Test]
        public void when_export_variable_should_be_equals_after_import()
        {
            var variable = new Variable(publicKey: Guid.NewGuid(), null)
            {
                Expression = "expression",
                Label = "Label",
                Name = "Name",
                DoNotExport = true,
                Type = VariableType.DateTime,
            };
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { variable }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_empty_static_text_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        Create.StaticText(enablementCondition: "enable"), 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
                          
        [Test]
        public void when_export_static_text_should_be_equals_after_import()
        {
            var staticText = new StaticText(
                publicKey: Guid.NewGuid(), 
                text: "text",
                conditionExpression: "",
                hideIfDisabled: true,
                attachmentName: "attachment #1",
                validationConditions: new List<ValidationCondition>()
                {
                    new ValidationCondition("val exp", "val message")
                });
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(
                    children: new [] { staticText }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        private QuestionnaireDocument DoImportExportQuestionnaire(QuestionnaireDocument questionnaireDocument, out IList<string> errors, Action<Questionnaire> postMappingChanges = null)
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new QuestionnaireAutoMapperProfile());
            }).CreateMapper();

            var service = new ImportExportQuestionnaireMapper(mapper);
            var questionnaire = service.Map(questionnaireDocument);
            postMappingChanges?.Invoke(questionnaire);
            var json = new QuestionnaireSerializer().Serialize(questionnaire);

            errors = ValidateBySchema(json);
            
            var newQuestionnaire = new QuestionnaireSerializer().Deserialize(json);
            var newDocument = service.Map(newQuestionnaire);
            return newDocument;
        }

        private IList<string> ValidateBySchema(string json)
        {
            var testType = typeof(QuestionnaireImportService);
            var readResourceFile = $"{testType.Namespace}.QuestionnaireSchema.json";

            using Stream stream = testType.Assembly.GetManifestResourceStream(readResourceFile);
            using StreamReader reader = new StreamReader(stream);
            string schemaText = reader.ReadToEnd();

            var schema = JSchema.Parse(schemaText);

            JToken jToken = JToken.Parse(json);
            jToken.IsValid(schema, out IList<ValidationError> errors);
            return errors.Select(e => e.Message).ToList();
        }
        
        private static Func<EquivalencyAssertionOptions<QuestionnaireDocument>, EquivalencyAssertionOptions<QuestionnaireDocument>> CompareOptions(
            Func<IMemberInfo, bool> ignoreFunc = null)
        {
            return opt => opt
                    .AllowingInfiniteRecursion()
                    .WithStrictOrdering()
                    .ThrowingOnMissingMembers()
                    .IncludingNestedObjects()
                    .IncludingProperties()
                    //.WithTracing()
                    .IgnoringCyclicReferences()
                    .ComparingEnumsByName()
                    .ComparingRecordsByMembers()
                    .IncludingAllDeclaredProperties()
                    .IncludingAllRuntimeProperties()
                    .Excluding(q => q.CreatedBy)
                    .Excluding(q => q.CreationDate)
                    .Excluding(q => q.LastEntryDate)
                    .Excluding(q => q.CoverPageSectionId)
                    //.Excluding((IMemberInfo mi) => mi.Name == nameof(IComposite.PublicKey))
                    .Excluding((IMemberInfo mi) => mi.Name == nameof(Group.RosterSizeQuestionId))
                    .Excluding((IMemberInfo mi) => mi.Name == nameof(Group.RosterTitleQuestionId))
                    .Excluding((IMemberInfo mi) => mi.Name == nameof(ICategoricalQuestion.CascadeFromQuestionId))
                    .Excluding((IMemberInfo mi) => mi.Name == nameof(ICategoricalQuestion.LinkedToQuestionId))
                    .Excluding((IMemberInfo mi) => mi.Name == nameof(ICategoricalQuestion.LinkedToRosterId))
                    //.Excluding((IMemberInfo mi) => mi.Name == nameof(Translation.Id))
                    //.Excluding((IMemberInfo mi) => mi.Name == nameof(Categories.Id))
                    .Excluding((IMemberInfo mi) => mi.Name == nameof(Attachment.AttachmentId))
                    .Excluding((IMemberInfo mi) => ignoreFunc != null && ignoreFunc.Invoke(mi))
                ;
        }
    }
}