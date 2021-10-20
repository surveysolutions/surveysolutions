using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
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
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1);
            questionnaireDocument.Title = "test";

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument);

            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire);
        }   
        
        [Test]
        public void when_export_questionnaire_with_info_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1);
            questionnaireDocument.Title = "test";
            questionnaireDocument.Description = "desc";
            questionnaireDocument.Id = "id";
            questionnaireDocument.Revision = 17;
            questionnaireDocument.CloseDate = DateTime.Now;
            //questionnaireDocument.ConditionExpression = "ConditionExpression";
            questionnaireDocument.CreatedBy = Guid.NewGuid();
            questionnaireDocument.CreationDate = DateTime.Now;
            questionnaireDocument.DefaultTranslation = Guid.NewGuid();
            questionnaireDocument.IsDeleted = true;
            questionnaireDocument.IsPublic = true;
            questionnaireDocument.OpenDate = DateTime.Now;
            questionnaireDocument.PublicKey = Guid.NewGuid();
            questionnaireDocument.VariableName = "VariableName";
            questionnaireDocument.DefaultLanguageName = "DefaultLanguageName";
            questionnaireDocument.HideIfDisabled = true;
            questionnaireDocument.LastEntryDate = DateTime.Now;
            //questionnaireDocument.LastEventSequence = 777;
            questionnaireDocument.CoverPageSectionId = Guid.NewGuid();
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

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument);

            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire);
        }
       
        [Test]
        public void when_export_empty_chapter_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                Create.Chapter()
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire);
        }
        
        [Test]
        public void when_export_one_chapter_should_be_equals_after_import()
        {
            var chapter = Create.Chapter(
                title: " Chapter",
                chapterId: Guid.NewGuid(),
                hideIfDisabled: true
            );
            chapter.Description = "Description";
            chapter.Enabled = true;
            chapter.ConditionExpression = "ConditionExpression";
            chapter.DisplayMode = RosterDisplayMode.Table;
            chapter.IsRoster = true;
            chapter.VariableName = "VariableName";
            chapter.CustomRosterTitle = true;
            chapter.HideIfDisabled = true;
            chapter.FixedRosterTitles = new FixedRosterTitle[]
            {
                new FixedRosterTitle(11, "rTitle11"),
                new FixedRosterTitle(22, "rTitle22"),
            };
            chapter.RosterSizeSource = RosterSizeSourceType.FixedTitles;
            chapter.RosterSizeQuestionId = Guid.NewGuid();
            chapter.RosterTitleQuestionId = Guid.NewGuid();

            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                chapter
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument);

            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire, 
                opt => opt.AllowingInfiniteRecursion()
                    .WithStrictOrdering()
                    .ThrowingOnMissingMembers()
                    .IncludingNestedObjects()
                    );
            questionnaireDocument.Children[0].Should().BeEquivalentTo(newQuestionnaire.Children[0]);
        }

        [Test]
        public void when_export_empty_text_question_should_be_equals_after_import()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(Id.g1,
                 Create.Chapter(
                     children: new []{Create.TextQuestion() }
                 )
            );

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire);
        }
        
        [Test]
        public void when_export_text_question_should_be_equals_after_import()
        {
            var textQuestion = Create.TextQuestion(
                questionId: Guid.NewGuid(),
                text: "Title",
                enablementCondition: "enablementCondition",
                validationExpression: "validationExpression",
                mask: "mask",
                variable: "variable",
                validationMessage: "validationMessage",
                scope: QuestionScope.Supervisor,
                preFilled: true,
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

            var newQuestionnaire = DoImportExportQuestionnaire(questionnaireDocument);
            
            questionnaireDocument.Should().BeEquivalentTo(newQuestionnaire);
        }

        private QuestionnaireDocument DoImportExportQuestionnaire(QuestionnaireDocument questionnaireDocument)
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new QuestionnaireAutoMapperProfile());
            }).CreateMapper();

            var service = new ImportExportQuestionnaireService(mapper, new NewtonJsonSerializer());
            var json = service.Export(questionnaireDocument);
            var newDocument = service.Import(json);
            return newDocument;
        }
    }
}