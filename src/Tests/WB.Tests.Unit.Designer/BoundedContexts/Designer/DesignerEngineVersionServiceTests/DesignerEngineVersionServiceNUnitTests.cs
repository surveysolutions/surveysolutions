﻿using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    [TestFixture]
    internal class DesignerEngineVersionServiceNUnitTests
    {
        private DesignerEngineVersionService CreateDesignerEngineVersionService()
        {
            return new DesignerEngineVersionService();
        }

        [Test]
        public void GetListOfNewFeaturesForClient_When_client_version_is_greater_then_10_Then_should_be_empty()
        {

            var designerEngineVersionService =this.CreateDesignerEngineVersionService();

            var result = designerEngineVersionService.GetListOfNewFeaturesForClient(Create.QuestionnaireDocument(), 11);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GetListOfNewFeaturesForClient_When_client_version_is_less_then_10_and_questionnaire_doesnt_have_hidden_questions_Then_should_be_empty()
        {

            var designerEngineVersionService = this.CreateDesignerEngineVersionService();

            var result = designerEngineVersionService.GetListOfNewFeaturesForClient(Create.QuestionnaireDocument(), 10);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GetListOfNewFeaturesForClient_When_client_version_is_less_then_10_and_questionnaire_has_hidden_question_Then_should_be_empty()
        {
            
            var designerEngineVersionService = this.CreateDesignerEngineVersionService();

            var result =
                designerEngineVersionService.GetListOfNewFeaturesForClient(
                    Create.QuestionnaireDocument(children:
                        new IComposite[] {Create.TextQuestion(scope: QuestionScope.Hidden)}), 9);
            
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetQuestionnaireContentVersion_When_questionnaire_contains_lookup_tables_Then_version_11_should_be_returned
           ()
        {
            var designerEngineVersionService =
                this.CreateDesignerEngineVersionService();
            var questionnaire = Create.QuestionnaireDocument();
            questionnaire.LookupTables.Add(Guid.Empty, Create.LookupTable("test"));
            var result =
                designerEngineVersionService.GetQuestionnaireContentVersion(questionnaire);

            Assert.That(result, Is.EqualTo(11));
        }

        [Test]
        public void GetQuestionnaireContentVersion_When_questionnaire_filtered_linked_question_Then_version_13_should_be_returned
          ()
        {
            var designerEngineVersionService =
                this.CreateDesignerEngineVersionService();
            var questionnaire = Create.QuestionnaireDocument(children:new [] {Create.SingleOptionQuestion(linkedFilterExpression:"aaa") });
            var result =
                designerEngineVersionService.GetQuestionnaireContentVersion(questionnaire);

            Assert.That(result, Is.EqualTo(13));
        }
    }
}