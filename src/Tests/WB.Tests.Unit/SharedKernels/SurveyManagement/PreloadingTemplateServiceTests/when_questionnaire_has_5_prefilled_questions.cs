using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadingTemplateServiceTests
{
    internal class when_questionnaire_has_5_prefilled_questions : PreloadingTemplateServiceTestContext
    {
        private Establish context = () =>
        {
            var sampleUploadView = Create.Entity.SampleUploadView(questionnaireId, 1, new List<FeaturedQuestionItem>()
            {
                Create.Entity.FeaturedQuestionItem(caption: "first name"),
                Create.Entity.FeaturedQuestionItem(caption: "last name"),
                Create.Entity.FeaturedQuestionItem(caption: "nationality"),
                Create.Entity.FeaturedQuestionItem(caption: "age"),
                Create.Entity.FeaturedQuestionItem(caption: "city"),
            });
            var sampleUploadViewFactory = Mock.Of<ISampleUploadViewFactory>(f => 
                f.Load(Moq.It.Is<SampleUploadViewInputModel>(m => m.QuestionnaireId == questionnaireId && m.Version == 1)) == sampleUploadView);

            preloadingTemplateService = CreatePreloadingTemplateServiceForGeneratePrefilledTemplate(sampleUploadViewFactory);
        };

        Because of = () => result = preloadingTemplateService.GetPrefilledPreloadingTemplateFile(questionnaireId, 1);

        It should_return_not_null_result = () =>
           result.ShouldNotBeNull();

        It should_return_template_with_all_prefilled_questions_in_header = () =>
        {
            var templateString = Encoding.UTF8.GetString(result);
            var columns = templateString.TrimEnd('\r', '\n').Split('\t');
            columns[0].ShouldEqual("first name");
            columns[1].ShouldEqual("last name");
            columns[2].ShouldEqual("nationality");
            columns[3].ShouldEqual("age");
            columns[4].ShouldEqual("city");
        };

        private static IPreloadingTemplateService preloadingTemplateService;
        private static byte[] result;
        private static readonly Guid questionnaireId = Guid.NewGuid();
    }
}
