using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.JsonExportServiceTests
{
    internal class when_getting_template_info : JsonExportServiceTestContext
    {
        Establish context = () =>
        {
            var questionnaireViewFactory = CreateQuestionnaireViewFactory();

            var versioner = Mock.Of<IQuestionnaireVersioner>(x => x.GetVersion(Moq.It.IsAny<QuestionnaireDocument>()) == version);
            exportService = CreateJsonExportService(questionnaireViewFactory: questionnaireViewFactory, versioner: versioner);
        };

        
        Because of = () =>
            info = exportService.GetQuestionnaireTemplateInfo(questionnaireId);

        It should_return_info_with_version = () => 
            info.Version.ShouldNotBeNull();

        It should_return_info_with_Major_equals_1 = () =>
            info.Version.Major.ShouldEqual(1);

        It should_return_info_with_Minor_equals_2 = () =>
            info.Version.Minor.ShouldEqual(2);

        It should_return_info_with_Patch_equals_3 = () =>
            info.Version.Patch.ShouldEqual(3);

        private static IJsonExportService exportService;
        private static QuestionnaireVersion version = new QuestionnaireVersion(1, 2, 3);
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static TemplateInfo info;
    }
}