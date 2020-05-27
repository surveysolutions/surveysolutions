﻿using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    [TestFixture]
    internal class DesignerEngineVersionServiceNUnitTests
    {
        private DesignerEngineVersionService CreateDesignerEngineVersionService(
            IAttachmentService attachments = null)
        {
            return new DesignerEngineVersionService(attachments ?? Mock.Of<IAttachmentService>());
        }

        [Test]
        public void should_return_27_when_supervisor_cascading_question_added()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.SingleOptionQuestion(scope: QuestionScope.Supervisor, cascadeFromQuestionId: Id.g1)
                );
            
            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);

            Assert.That(contentVersion, Is.EqualTo(27));
        }

        [Test]
        public void should_return_version_24_when_non_image_attachment_exists()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter();
            var contentId = "contentId";
            questionnaire.Attachments.Add(Create.Attachment(Id.gA, contentId: contentId));

            var attachmentContent = Create.AttachmentContent(contentType: "video/mp4", contentId: contentId);

            var attachmentService = Mock.Of<IAttachmentService>(x => x.GetContent(contentId) == attachmentContent);

            var service = this.CreateDesignerEngineVersionService(attachmentService);
 
            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
 
            Assert.That(contentVersion, Is.EqualTo(24));
        }

        [Test]
        public void should_return_version_25_when_section_has_variable_name()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.Group(variable:"test")
            });
            

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(25));
        }

        [Test]
        public void should_return_version_26_when_contains_multioption_as_combobox()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.MultyOptionsQuestion(filteredCombobox:true)
                });


            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(26));
        }

        [Test]
        public void should_return_version_26_when_contains_singleoption_show_as_list()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.SingleOptionQuestion(showAsList: true)
                });


            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(26));
        }

        [Test]
        public void should_return_27_when_self_is_used_in_substitutions()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.SingleOptionQuestion(title: "%self%")
                });


            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(27));
        }

        [Test]
        public void should_return_27_when_table_roster_is_used()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.Roster(displayMode: RosterDisplayMode.Table)
                });


            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(27));
        }


        [Test]
        public void should_return_29_when_linked_to_list_question_with_option_filter()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.TextListQuestion(Id.g1),
                    Create.MultipleOptionsQuestion(optionsFilterExpression: "filter", linkedToQuestionId: Id.g1),
                });


            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(29));
        }

        [Test]
        public void should_not_return_29_when_question_with_option_filter()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.SingleOptionQuestion(optionsFilterExpression: "filter"),
                });

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.Not.EqualTo(29));
        }
        
        [Test]
        public void should_return_29_when_custom_title_used()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                    Create.Roster(customRosterTitle: true)
            );


            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(29));
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

    }
}
