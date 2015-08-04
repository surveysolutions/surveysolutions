using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Designer.Api;
using WB.UI.Shared.Web.Membership;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.ImportControllerTests
{
    internal class when_getting_Questionaire_and_questionnaire_is_valid_has_one_numeric_roster_size_question : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);

            var membershipUserService = Mock.Of<IMembershipUserService>(
                _ => _.WebUser == Mock.Of<IMembershipWebUser>(
                    u => u.UserId == userId));

            var numericIntQuestion = Create.NumericIntegerQuestion(id: numericIntQuestionId);
            numericIntQuestion.MaxValue = 3;
            
            var questionnaireDocument = Create.QuestionnaireDocument(children: new IComposite[]
            {
                numericIntQuestion,
                Create.NumericQuestion(questionId: numericRealQuestionId, isInteger: false),
                Create.NumericIntegerQuestion(id: rosterSizeQuestionId),
                Create.Roster(rosterSizeQuestionId: rosterSizeQuestionId,
                    rosterSizeSourceType: RosterSizeSourceType.Question)
            });
            questionnaireDocument.CreatedBy = userId;

            var questionnaireViewFactory = Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(
                _ => _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == Create.QuestionnaireView(questionnaireDocument));

            var expressionsEngineVersionService = Mock.Of<IExpressionsEngineVersionService>(
                _ => _.IsClientVersionSupported(Moq.It.IsAny<Version>()) == true);

            var questionnaireVerifier = Mock.Of<IQuestionnaireVerifier>(
                _ => _.Verify(Moq.It.IsAny<QuestionnaireDocument>()) == new QuestionnaireVerificationError[0]);

            string generatedAssembly = "test";
            var expressionProcessorGenerator = Mock.Of<IExpressionProcessorGenerator>(
                _ =>
                    _.GenerateProcessorStateAssembly(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<Version>(),
                        out generatedAssembly) == Create.GenerationResult(true));

            var jsonUtilsMock=new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Serialize(Moq.It.IsAny<object>())).Returns("").Callback<object>(d => resultQuestionnaire = d as QuestionnaireDocument);

            importController = CreateImportController(membershipUserService: membershipUserService,
                questionnaireViewFactory: questionnaireViewFactory,
                expressionsEngineVersionService: expressionsEngineVersionService,
                questionnaireVerifier: questionnaireVerifier,
                expressionProcessorGenerator: expressionProcessorGenerator,
                jsonUtils: jsonUtilsMock.Object);
        };

        Because of = () =>
            questionnaireCommunicationPackage = importController.Questionnaire(request);

        It should_return_not_null_responce = () =>
            questionnaireCommunicationPackage.ShouldNotBeNull();

        It should_return_questionnaire_document_with_MaxValue_of_roster_size_question_equal_to_40 = () =>
            resultQuestionnaire.FirstOrDefault<NumericQuestion>(q => q.PublicKey == rosterSizeQuestionId)
                .MaxValue.Value.ShouldEqual(Constants.MaxRosterRowCount);

        It should_return_questionnaire_document_with_MaxValue_of_numeric_int_question_equal_to_null = () =>
            resultQuestionnaire.FirstOrDefault<NumericQuestion>(q => q.PublicKey == numericIntQuestionId)
                .MaxValue.ShouldBeNull();

        It should_return_questionnaire_document_with_MaxValue_of_numeric_real_question_equal_to_null = () =>
           resultQuestionnaire.FirstOrDefault<NumericQuestion>(q => q.PublicKey == numericRealQuestionId)
               .MaxValue.ShouldBeNull();

        private static ImportController importController;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
        private static QuestionnaireCommunicationPackage questionnaireCommunicationPackage;
        private static QuestionnaireDocument resultQuestionnaire;
        private static Guid numericIntQuestionId = Guid.NewGuid();
        private static Guid numericRealQuestionId = Guid.NewGuid();
        private static Guid rosterSizeQuestionId = Guid.NewGuid();
    }
}