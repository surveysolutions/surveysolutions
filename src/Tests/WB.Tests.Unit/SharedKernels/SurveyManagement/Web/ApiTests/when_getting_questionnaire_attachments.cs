﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_getting_questionnaire_attachments : ApiTestContext
    {
        private Establish context = () =>
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
        };

        Because of = () =>
        {
            responseMessage = controller.GetAttachments(questionnairesId, questionnairesVersion);
        };

        It should_return_HttpResponseMessage = () =>
            responseMessage.ShouldBeOfExactType<HttpResponseMessage>();

        It should_call_get_questionnaire_document_once = () =>
            Mock.Get(questionnaireStorage).Verify(x => x.GetQuestionnaireDocument(questionnairesId, questionnairesVersion), Times.Once());

        private It should_return_content_in_body = () =>
            responseMessage.Content.ReadAsAsync<List<string>>().Result.ShouldEqual(attachments.Select(a => a.ContentId).ToList());



        private static Guid questionnairesId = Guid.NewGuid();
        private static int questionnairesVersion = 7;
        private static Attachment[] attachments;
        private static QuestionnaireDocument questionnaireDocument; 
        private static HttpResponseMessage responseMessage;
        private static QuestionnairesApiV2Controller controller;
        private static IQuestionnaireStorage questionnaireStorage;
    }
}
