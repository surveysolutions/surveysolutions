using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Tests.Abc.Storage;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2;
using WB.UI.Headquarters.Models.CompanyLogo;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewerApiTests
{
    [TestFixture]
    [TestOf(typeof(SettingsV2Controller))]
    public class CompanyLogoApiV2ControllerTestsL
    {
        [Test]
        public void when_headquarters_has_no_logo_Should_return_NoContent_response()
        {
            var controller = this.GetController();

            // act
            var response = controller.CompanyLogo();
            
            // assert
            Assert.That(response, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public void when_there_is_logo_on_headquarters_and_no_etag_provided()
        {
            CompanyLogo logo = Web.Create.Entity.HqCompanyLogo();
            IPlainKeyValueStorage<CompanyLogo> logoStorage = new InMemoryKeyValueStorage<CompanyLogo>();
            logoStorage.Store(logo, CompanyLogo.CompanyLogoStorageKey);

            var controller = this.GetController(logoStorage);

            // act
            var response = controller.CompanyLogo();

            // assert
            Assert.That(response, Is.InstanceOf<FileContentResult>());
            var fileResult = (FileContentResult) response;
            var expectedEtag = $"\"{logo.GetEtagValue()}\"";
            Assert.That(fileResult.EntityTag.ToString(), Is.EqualTo(expectedEtag));
            Assert.That(fileResult.FileContents, Is.EquivalentTo(logo.Logo));
        }

        [Test]
        public void when_company_logo_has_not_changed_Should_return_NotModified_response()
        {
            CompanyLogo logo = Web.Create.Entity.HqCompanyLogo();
            IPlainKeyValueStorage<CompanyLogo> logoStorage = new InMemoryKeyValueStorage<CompanyLogo>();
            logoStorage.Store(logo, CompanyLogo.CompanyLogoStorageKey);

            var controller = this.GetController(logoStorage, requestEtag: $"\"{logo.GetEtagValue()}\"");

            // act
            var response = controller.CompanyLogo();

            // assert
            Assert.That(response, Is.InstanceOf<StatusCodeResult>()
                                    .With.Property(nameof(StatusCodeResult.StatusCode))
                                         .EqualTo(StatusCodes.Status304NotModified));
        }

        SettingsV2Controller GetController(IPlainKeyValueStorage<CompanyLogo> logoStorage = null,
            string requestEtag = null, IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage = null)
        {
            var companyLogoApiV2Controller = new SettingsV2Controller(logoStorage ?? new InMemoryKeyValueStorage<CompanyLogo>(),
                interviewerSettingsStorage ?? new InMemoryKeyValueStorage<InterviewerSettings>(),
                new TestPlainStorage<ServerSettings>(), 
                Mock.Of<ISecureStorage>(),
                Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(), 
                Mock.Of<IWebInterviewLinkProvider>());

            var defaultHttpContext = new DefaultHttpContext();
            companyLogoApiV2Controller.ControllerContext  = new ControllerContext
            {
                HttpContext = defaultHttpContext
            };
            if (requestEtag != null)
            {
                defaultHttpContext.Request.Headers[HeaderNames.IfNoneMatch] = requestEtag;
            }
            
            return companyLogoApiV2Controller;
        }
    }
}
