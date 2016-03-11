using Machine.Specifications;
using WB.UI.Designer.WebServices;
using WB.UI.Designer.WebServices.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.Applications.PublicServiceTests
{
    internal class when_service_and_response_objects_have_specific_namespace : PublicServiceTestContext
    {
        It should_PublicService_has_specific_namespace = () =>
            typeof(PublicService).Namespace.ShouldEqual(serviceNamespace);

        It should_IPublicService_placed_in_specific_namespace = () =>
            typeof(IPublicService).Namespace.ShouldEqual(serviceNamespace);

        It should_RemoteFileInfo_placed_in_specific_namespace = () =>
            typeof(RemoteFileInfo).Namespace.ShouldEqual(serviceNamespace);

        It should_DownloadQuestionnaireRequest_placed_in_specific_namespace = () =>
            typeof(DownloadQuestionnaireRequest).Namespace.ShouldEqual(serviceQuestionnaireNamespace);

        It should_QuestionnaireListRequest_placed_in_specific_namespace = () =>
            typeof(QuestionnaireListRequest).Namespace.ShouldEqual(serviceQuestionnaireNamespace);

        It should_QuestionnaireListViewItemMessage_placed_in_specific_namespace = () =>
            typeof(QuestionnaireListViewItemMessage).Namespace.ShouldEqual(serviceQuestionnaireNamespace);

        It should_QuestionnaireListViewMessage_placed_in_specific_namespace = () =>
            typeof(QuestionnaireListViewMessage).Namespace.ShouldEqual(serviceQuestionnaireNamespace);


        private static string serviceNamespace = "WB.UI.Designer.WebServices";
        private static string serviceQuestionnaireNamespace = string.Format("{0}.Questionnaire", serviceNamespace);
    }
}
