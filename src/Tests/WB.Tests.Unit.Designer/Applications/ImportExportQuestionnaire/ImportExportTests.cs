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
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;
using WB.UI.Designer.Code.ImportExport;

namespace WB.Tests.Unit.Designer.Applications.ImportExportQuestionnaire
{
    [TestOf(typeof(ImportExportQuestionnaireService))]
    public class ImportExportTests
    {
        [Test]
        public void when_export_empty_questionnaire_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1, children: new[]
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
            questionnaireDocument.DefaultTranslation = Guid.NewGuid();
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
                new Group() { PublicKey = coverPageSectionId }
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
                new Group() { PublicKey = coverPageSectionId }
            }.ToReadOnlyCollection();

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);

            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            errors.Count.Should().Be(0);
        }
       
        [Test]
        public void when_export_empty_chapter_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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

            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                chapter
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);

            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            questionnaireDocument.Children[0].Should().BeEquivalentTo(newQuestionnaire.Children[0]);
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
            roster.CustomRosterTitle = true;
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
                children: new []{ roster }
            );
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                chapter
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);

            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            questionnaireDocument.Children[0].Should().BeEquivalentTo(newQuestionnaire.Children[0]);
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_questionnaire_with_attachment_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter()
            );
            questionnaireDocument.Attachments = new List<Attachment>()
            {
                new Attachment() { Name = "attach #1", AttachmentId = Guid.NewGuid()/*, ContentId = "content1"*/},
                new Attachment() { Name = "attach #2", AttachmentId = Guid.NewGuid()/*, ContentId = "content2"*/},
            };

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_questionnaire_with_categories_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter()
            );
            questionnaireDocument.Categories = new List<Categories>()
            {
                new Categories() { Name = "Categories #1", Id = Guid.NewGuid() },
                new Categories() { Name = "Categories #2", Id = Guid.NewGuid() },
            };

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_questionnaire_with_macroses_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter()
            );
            questionnaireDocument.Macros = new Dictionary<Guid, Macro>()
            {
                { Guid.NewGuid(), new Macro() { Name = "Macro #1", Description = "Description 1", Content = "content1"} },
                { Guid.NewGuid(), new Macro() { Name = "Macro #2", Description = "Description 2", Content = "content2"} },
            };

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_questionnaire_with_translations_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter()
            );
            questionnaireDocument.Translations = new List<Translation>()
            {
                new Translation() { Name = "Translation #1", Id = Guid.NewGuid() },
                new Translation() { Name = "Translation #2", Id = Guid.NewGuid() },
            };

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
        
        [Test]
        public void when_export_questionnaire_with_lookup_tables_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter()
            );
            questionnaireDocument.LookupTables = new Dictionary<Guid, LookupTable>()
            {
                { Guid.NewGuid(), new LookupTable() { FileName = "FileName #1", TableName = "TableName 1", } },
                { Guid.NewGuid(), new LookupTable() { FileName = "FileName #2", TableName = "TableName 2", } },
            };

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        [Test]
        public void when_export_empty_text_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                 Create.Chapter(
                     children: new []{Create.TextQuestion() }
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
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                 Create.Chapter(
                     children: new []
                     {
                         Create.NumericIntegerQuestion(variable: "int"), 
                         Create.NumericRealQuestion(variable: "real")
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
                options: new []{ new Option("1", "option1", "1") }
                );
            numericQuestion.CountOfDecimalPlaces = 7;
            numericQuestion.UseFormatting = true;
            numericQuestion.Properties = new QuestionProperties(hideInstructions: true, useFormatting: true);
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new AudioQuestion() { QuestionType = QuestionType.Audio }, 
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
                QuestionType = QuestionType.Audio,
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                //Featured = true,
                HideIfDisabled = true,
            };
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new AreaQuestion() { QuestionType = QuestionType.Area }, 
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
                QuestionType = QuestionType.Area,
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                //Featured = true,
                HideIfDisabled = true,
                Properties = new QuestionProperties(hideInstructions: true, useFormatting: false)
                {
                    GeometryType = GeometryType.Polygon,
                }
            };
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new DateTimeQuestion() { QuestionType = QuestionType.DateTime }, 
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
                QuestionType = QuestionType.DateTime,
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                //Featured = true,
                HideIfDisabled = true,
                IsTimestamp = true,
                Properties = new QuestionProperties(hideInstructions: true, useFormatting: false)
                {
                    DefaultDate = DateTime.Now
                }
            };
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new GpsCoordinateQuestion() { QuestionType = QuestionType.GpsCoordinates }, 
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
                QuestionType = QuestionType.GpsCoordinates,
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                //Featured = true,
                HideIfDisabled = true,
            };
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new MultimediaQuestion() { QuestionType = QuestionType.Multimedia }, 
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
                QuestionType = QuestionType.Multimedia,
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                //Featured = true,
                HideIfDisabled = true,
                IsSignature = true
            };
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new MultyOptionsQuestion() { QuestionType = QuestionType.MultyOption }, 
                    }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }
                   
        [Test]
        public void when_export_mutioptions_question_should_be_equals_after_import()
        {
            var question = new MultyOptionsQuestion()
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = Id.g7,
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                //Featured = true,
                HideIfDisabled = true,
                AreAnswersOrdered = true,
                MaxAllowedAnswers = 7,
                YesNoView = true,
                CategoriesId = Guid.NewGuid(),
                Answers = new List<Answer>()
                {
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
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new QRBarcodeQuestion() { QuestionType = QuestionType.QRBarcode }, 
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
                QuestionType = QuestionType.QRBarcode,
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                //Featured = true,
                HideIfDisabled = true,
            };
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new SingleQuestion() { QuestionType = QuestionType.SingleOption }, 
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
                QuestionType = QuestionType.SingleOption,
                PublicKey = Id.g7,
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                //Featured = true,
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
                IsFilteredCombobox = true,
            };
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
        public void when_export_empty_text_list_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        new TextListQuestion() { QuestionType = QuestionType.TextList }, 
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
                QuestionType = QuestionType.TextList,
                PublicKey = Guid.NewGuid(),
                QuestionText = "question title",
                VariableLabel = "variable",
                ConditionExpression = "enablementCondition",
                ValidationConditions = new[] { Create.ValidationCondition() },
                QuestionScope = QuestionScope.Supervisor,
                //Featured = true,
                HideIfDisabled = true,
                MaxAnswerCount = 77,
            };
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter(
                    children: new []
                    {
                        Create.StaticText(), 
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
            
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter(
                    children: new [] { staticText }
                )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument, out var errors);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, CompareOptions());
            newQuestionnaire.Should().BeEquivalentTo(questionnaireDocument, CompareOptions());
            errors.Count.Should().Be(0);
        }

        private QuestionnaireDocument DoImportExportQuestionnaire(QuestionnaireDocument questionnaireDocument, out IList<string> errors)
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new QuestionnaireAutoMapperProfile());
            }).CreateMapper();

            var service = new ImportExportQuestionnaireService(mapper, new NewtonJsonSerializer());
            var json = service.Export(questionnaireDocument);

            errors = ValidateBySchema(json);
            
            var newDocument = service.Import(json);
            return newDocument;
        }

        private IList<string> ValidateBySchema(string json)
        {
            var testType = typeof(ImportExportTests);
            var readResourceFile = $"{testType.Namespace}.SchemaExample.json";

            using Stream stream = testType.Assembly.GetManifestResourceStream(readResourceFile);
            using StreamReader reader = new StreamReader(stream);
            string schemaText = reader.ReadToEnd();

            var schema = JSchema.Parse(schemaText);

            JToken jToken = JToken.Parse(json);
            jToken.IsValid(schema, out IList<ValidationError> errors);
            return errors.Select(e => e.Message).ToList();
        }
        
        private static Func<EquivalencyAssertionOptions<QuestionnaireDocument>, EquivalencyAssertionOptions<QuestionnaireDocument>> CompareOptions()
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
                ;
        }
    }
}