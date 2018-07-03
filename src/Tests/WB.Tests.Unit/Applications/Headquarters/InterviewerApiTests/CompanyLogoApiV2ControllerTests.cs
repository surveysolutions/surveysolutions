using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v2;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.Tests.Unit.Applications.Headquarters.InterviewerApiTests
{
    [TestFixture]
    [TestOf(typeof(SettingsV2Controller))]
    public class CompanyLogoApiV2ControllerTests
    {
        [Test]
        public void when_headquarters_has_no_logo_Should_return_NoContent_reponse()
        {
            var controller = this.GetController();

            // act
            var response = controller.CompanyLogo();
            
            // assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(response.Content, Is.Null);
        }

        [Test]
        public async Task when_there_is_logo_on_headquarters_and_no_etag_provided()
        {
            CompanyLogo logo = Create.Entity.HqCompanyLogo();
            IPlainKeyValueStorage<CompanyLogo> logoStorage = new InMemoryKeyValueStorage<CompanyLogo>();
            logoStorage.Store(logo, CompanyLogo.CompanyLogoStorageKey);

            var controller = this.GetController(logoStorage);

            // act
            var response = controller.CompanyLogo();

            // assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.ETag.Tag, Is.EqualTo($"\"{logo.GetEtagValue()}\""), "Etag of the logo should be provided in response");
            Assert.That(response.Content, Is.InstanceOf(typeof(ByteArrayContent)));

            var byteArrayContent = (ByteArrayContent)response.Content;
            var byteArray = await byteArrayContent.ReadAsByteArrayAsync();
            Assert.That(byteArray, Is.EquivalentTo(logo.Logo));
        }

        [Test]
        public void when_company_logo_has_not_changed_Should_return_NotModified_response()
        {
            CompanyLogo logo = Create.Entity.HqCompanyLogo();
            IPlainKeyValueStorage<CompanyLogo> logoStorage = new InMemoryKeyValueStorage<CompanyLogo>();
            logoStorage.Store(logo, CompanyLogo.CompanyLogoStorageKey);

            var controller = this.GetController(logoStorage, requestEtag: $"\"{logo.GetEtagValue()}\"");

            // act
            var response = controller.CompanyLogo();

            // assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotModified));
            Assert.That(response.Content, Is.Null);
        }

        SettingsV2Controller GetController(IPlainKeyValueStorage<CompanyLogo> logoStorage = null,
            string requestEtag = null, IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage = null)
        {
            var companyLogoApiV2Controller = new SettingsV2Controller(logoStorage ?? new InMemoryKeyValueStorage<CompanyLogo>(),
                interviewerSettingsStorage ?? new InMemoryKeyValueStorage<InterviewerSettings>());
            var httpRequestMessage = new HttpRequestMessage();
            if (requestEtag != null)
            {
                httpRequestMessage.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(requestEtag));
            }

            companyLogoApiV2Controller.Request = httpRequestMessage;
            return companyLogoApiV2Controller;
        }
    }
}
