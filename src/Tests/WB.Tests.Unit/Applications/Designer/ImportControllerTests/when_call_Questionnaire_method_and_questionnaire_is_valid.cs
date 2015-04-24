using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api;
using WB.UI.Shared.Web.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.ImportControllerTests
{
    internal class when_call_Questionnaire_method_and_questionnaire_is_valid : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = new DownloadQuestionnaireRequest()
            {
                QuestionnaireId = questionnaireId,
                SupportedVersion = new QuestionnnaireVersion()
            };

            var membershipUserService =
                Mock.Of<IMembershipUserService>(
                    _ => _.WebUser == Mock.Of<IMembershipWebUser>(u => u.UserId == userId));

            var questionnaireViewFactory =
                Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(
                    _ =>
                        _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) ==
                        new QuestionnaireView(new QuestionnaireDocument() { CreatedBy = userId }));

            var expressionsEngineVersionService =
                Mock.Of<IExpressionsEngineVersionService>(
                    _ => _.IsClientVersionSupported(Moq.It.IsAny<Version>()) == true);

            var questionnaireVerifier =
                Mock.Of<IQuestionnaireVerifier>(
                    _ =>
                        _.Verify(Moq.It.IsAny<QuestionnaireDocument>()) == new QuestionnaireVerificationError[0]);

            string generatedAssembly="test";
            var expressionProcessorGenerator =
                Mock.Of<IExpressionProcessorGenerator>(
                    _ =>
                        _.GenerateProcessorStateAssembly(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<Version>(),
                            out generatedAssembly) == new GenerationResult() { Success = true });

            importController = CreateImportController(membershipUserService: membershipUserService,
                questionnaireViewFactory: questionnaireViewFactory,
                expressionsEngineVersionService: expressionsEngineVersionService,
                questionnaireVerifier: questionnaireVerifier,
                expressionProcessorGenerator: expressionProcessorGenerator);
        };

        Because of = () =>
               questionnaireCommunicationPackage= importController.Questionnaire(request);

        It QuestionnaireCommunicationPackage_should_not_be_null = () =>
            questionnaireCommunicationPackage.ShouldNotBeNull();

        private static ImportController importController;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
        private static QuestionnaireCommunicationPackage questionnaireCommunicationPackage;
    }
}
