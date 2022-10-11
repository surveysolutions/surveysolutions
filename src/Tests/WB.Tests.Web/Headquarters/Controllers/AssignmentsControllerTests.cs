/*
using System;
using AutoMapper;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Controllers.Api.PublicApi;
using AssignmentAnswer = WB.UI.Headquarters.Controllers.Api.PublicApi.AssignmentAnswer;

namespace WB.Tests.Web.Headquarters.Controllers;

public class AssignmentsControllerTests
{
    [Test]
    public void convert_()
    {
        var controller = new AssignmentsController(
            Mock.Of<IAssignmentViewFactory>(),
            Mock.Of<IAssignmentsService>(),
            Mock.Of<IMapper>(),
            Mock.Of<IUserRepository>(),
            Mock.Of<IQuestionnaireStorage>(),
            Mock.Of<ISystemLog>(),
            Mock.Of<IPreloadedDataVerifier>(),
            Mock.Of<ICommandService>(),
            Mock.Of<IAuthorizedUser>(),
            Mock.Of<IUnitOfWork>(),
            Mock.Of<IUserViewFactory>(),
            Mock.Of<IAssignmentsImportService>(),
            new NewtonJsonSerializer(),
            Mock.Of<IInvitationService>(),
            Mock.Of<IWebInterviewLinkProvider>(),
            Mock.Of<IInScopeExecutor>());

        var questionPublicKey = "11111111111111111111111111111111";
        var questionnaire = WB.Tests.Abc.Create.Entity.PlainQuestionnaire(
            WB.Tests.Abc.Create.Entity.QuestionnaireDocument(
                    children: new[]{ new TextListQuestion()
                    {
                        PublicKey = Guid.Parse(questionPublicKey),
                        StataExportCaption = "test1"
                    }}));
        
        AssignmentAnswer answer = new AssignmentAnswer(
            new AssignmentIdentifyingDataItem()
            {
                Answer = "[\"test\", \"test1\"]",
                Identity = questionPublicKey,
                Variable = "test1"
            },
            new Identity(Guid.NewGuid(),RosterVector.Empty )){Variable = "test1"};

        
        var answerValue = controller.ToPreloadTextListAnswer(answer, questionnaire) as AssignmentAnswers;

        Assert.That(answerValue.Values.Length, Is.EqualTo(2));

    }
}
*/
