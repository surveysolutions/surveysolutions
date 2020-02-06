using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Main.Core.Documents;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_getting_questionnaire_attachments : ApiTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            attachments = new[]
            {
                Create.Entity.Attachment("1"),
                Create.Entity.Attachment("3"),
                Create.Entity.Attachment("2"),
                Create.Entity.Attachment("5"),
            };
            questionnaireDocument = Create.Entity.QuestionnaireDocumentWithAttachments(chapterId: Guid.NewGuid(), attachments: attachments);

            questionnaireStorage = Mock.Of<IQuestionnaireStorage>(
                x => x.GetQuestionnaireDocument(questionnairesId, questionnairesVersion) == questionnaireDocument);

            controller = CreateQuestionnairesApiV2Controller(questionnaireStorage: questionnaireStorage);
            BecauseOf();
        }

        public void BecauseOf()
        {
            responseMessage = controller.GetAttachments(questionnairesId, questionnairesVersion);
        }

        [NUnit.Framework.Test]
        public void should_call_get_questionnaire_document_once() =>
            Mock.Get(questionnaireStorage).Verify(x => x.GetQuestionnaireDocument(questionnairesId, questionnairesVersion), Times.Once());

        [NUnit.Framework.Test]
        public void should_return_content_in_body() => 
            responseMessage.Value.Should().BeEquivalentTo(attachments.Select(a => a.ContentId).ToList());


        private static Guid questionnairesId = Guid.NewGuid();
        private static int questionnairesVersion = 7;
        private static Attachment[] attachments;
        private static QuestionnaireDocument questionnaireDocument;
        private static ActionResult<List<string>> responseMessage;
        private static QuestionnairesApiV2Controller controller;
        private static IQuestionnaireStorage questionnaireStorage;
    }
}
