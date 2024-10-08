﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    [TestFixture]
    [TestOf(typeof(DesignerEngineVersionService))]
    internal class DesignerEngineVersionServiceNUnitTests
    {
        private DesignerEngineVersionService CreateDesignerEngineVersionService(
            IAttachmentService attachments = null,
            IDesignerTranslationService translationsService = null,
            IReusableCategoriesService reusableCategoriesService = null)
        {
            return new DesignerEngineVersionService(attachments ?? Mock.Of<IAttachmentService>(),
                translationsService ?? Mock.Of<IDesignerTranslationService>(),
                reusableCategoriesService ?? Mock.Of<IReusableCategoriesService>());
        }

        [Test]
        public void should_return_30_when_cover_page_used()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithCoverPage();

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(30));
        }

        [Test]
        public void should_return_31_when_static_text_references_variable()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithCoverPage(children:new IComposite[]
            {
                Create.StaticText(attachmentName: "var1"),
                Create.Variable(variableName: "var1", type: VariableType.String)
            });

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            
            Assert.That(contentVersion, Is.EqualTo(31));
        }

        [Test]
        public void should_return_31_when_has_translated_title()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter();
            questionnaire.Translations.Add(new Translation
            {
                Id = Id.gA
            });
            var dbContext = Create.InMemoryDbContext();
            dbContext.TranslationInstances.Add(Create.TranslationInstance(questionnaire.PublicKey,
                TranslationType.Title, questionnaire.PublicKey, translationId: Id.gA));

            dbContext.SaveChanges();

            var translationsService = Create.TranslationsService(dbContext);
            var service = CreateDesignerEngineVersionService(translationsService: translationsService);

            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);

            Assert.That(contentVersion, Is.EqualTo(31));
        }

        [Test]
        public void should_return_32_when_linked_to_roster_question_is_filtered_combobox()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.FixedRoster(Id.g1),
                Create.SingleQuestion(linkedToRosterId: Id.g1, isFilteredCombobox: true));
            
            var service = CreateDesignerEngineVersionService();

            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);

            Assert.That(contentVersion, Is.EqualTo(32));
        }
        
        [Test]
        public void should_return_32_when_linked_to_question_question_is_filtered_combobox()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.TextListQuestion(Id.g1),
                Create.SingleQuestion(linkedToQuestionId: Id.g1, isFilteredCombobox: true));
            
            var service = CreateDesignerEngineVersionService();

            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);

            Assert.That(contentVersion, Is.EqualTo(32));
        }
        
        [Test]
        public void should_return_33_when_attachment_in_option()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.SingleOptionQuestion(answers: new List<Answer>(){new Answer(){AttachmentName = "test"}}),
                });

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(33));
        }
        
        [Test]
        public void should_return_33_when_attachment_in_reusable_categories()
        {
            var categoryId = Id.g1;
            
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.SingleOptionQuestion(categoriesId:categoryId),
                });

            questionnaire.Categories = new List<Categories>()
            {
                new Categories() { Id = categoryId, Name = "category1"},
            };

            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoryId) == new[]
                {
                    new CategoriesItem {Id = 1, Text = "opt 1", AttachmentName = "test"}
                }.AsQueryable());

            var service = this.CreateDesignerEngineVersionService(reusableCategoriesService: categoriesService);

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(33));
        }
        
        [Test]
        public void should_return_30_when_no_attachment_in_reusable_categories()
        {
            var categoryId = Id.g1;
            
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.SingleOptionQuestion(categoriesId:categoryId),
                });

            questionnaire.Categories = new List<Categories>()
            {
                new Categories() { Id = categoryId, Name = "category1"},
            };

            var categoriesService = Mock.Of<IReusableCategoriesService>(x =>
                x.GetCategoriesById(It.IsAny<Guid>(), categoryId) == new[]
                {
                    new CategoriesItem {Id = 1, Text = "opt 1"}
                }.AsQueryable());

            var service = this.CreateDesignerEngineVersionService(reusableCategoriesService: categoriesService);

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(30));
        }
        
        [Test]
        public void should_return_34_when_geography_input_mode_is_auto()
        {
            var categoryId = Id.g1;
            
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.Question(questionType:QuestionType.Area, 
                        properties:new QuestionProperties(false,false)
                        {
                            GeometryInputMode = GeometryInputMode.Automatic
                        }),
                });

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(34));
        }
        
        [Test]
        public void should_return_34_when_geography_show_neighbours_is_on()
        {
            var categoryId = Id.g1;
            
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.Question(questionType:QuestionType.Area, 
                        properties:new QuestionProperties(false,false)
                        {
                            GeometryOverlapDetection = true
                        }),
                });

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(34));
        }
        
        [Test]
        public void should_return_34_when_geography_accuracy_is_used_in_validation()
        {
            var categoryId = Id.g1;
            
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.Question(questionType:QuestionType.Area,
                        variable: "geo",
                        validationConditions: new ValidationCondition[]
                        {
                          new ValidationCondition("geo.RequestedAccuracy > 3", "test")  
                        },
                        properties:new QuestionProperties(false,false)
                        {
                            GeometryOverlapDetection = false
                        }),
                });

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(34));
        }
        
        [Test]
        public void should_return_34_when_geography_accuracy_is_used_in_validation_as_self()
        {
            var categoryId = Id.g1;
            
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.Question(questionType:QuestionType.Area,
                        variable: "geo",
                        validationConditions: new ValidationCondition[]
                        {
                            new ValidationCondition("self.  RequestedAccuracy > 3", "test")  
                        },
                        properties:new QuestionProperties(false,false)
                        {
                            GeometryOverlapDetection = false
                        }),
                });

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(34));
        }
        
        [Test]
        public void should_return_34_when_geography_frequency_is_used_in_validation()
        {
            var categoryId = Id.g1;
            
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.Question(questionType:QuestionType.Area,
                        variable: "geo",
                        validationConditions: new ValidationCondition[]
                        {
                            new ValidationCondition("geo.RequestedFrequency > 11", "test")  
                        },
                        properties:new QuestionProperties(false,false)
                        {
                            GeometryOverlapDetection = false
                        }),
                });

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(34));
        }
        
        [Test]
        public void should_return_36_when_zero_is_used_for_CountOfDecimalPlaces()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.NumericRealQuestion(Id.g1, decimalPlaces: 0));
            
            var service = CreateDesignerEngineVersionService();
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            
            Assert.That(contentVersion, Is.EqualTo(36));
        }
    }
}
