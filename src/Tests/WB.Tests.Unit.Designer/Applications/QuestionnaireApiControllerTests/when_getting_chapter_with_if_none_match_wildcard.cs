using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_getting_chapter_with_if_none_match_wildcard : QuestionnaireApiControllerTestContext
    {
        [Test]
        public async Task should_return_not_modified_and_cache_validation_headers()
        {
            var dbContext = Create.InMemoryDbContext();
            dbContext.QuestionnaireChangeRecords.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.QuestionnaireId.FormatGuid(),
                sequence: 7));
            dbContext.SaveChanges();

            var controller = CreateQuestionnaireController(dbContext: dbContext);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var etag = $"\"{questionnaireId.QuestionnaireId:N}_7\"";
            controller.Request.Headers.IfNoneMatch = "*";

            var actionResult = await controller.Chapter(questionnaireId, "chapter-id");

            Assert.That(actionResult, Is.InstanceOf<StatusCodeResult>());
            Assert.That(((StatusCodeResult)actionResult).StatusCode, Is.EqualTo(StatusCodes.Status304NotModified));
            Assert.That(controller.Response.Headers.ETag.ToString(), Is.EqualTo(etag));
            Assert.That(controller.Response.Headers.CacheControl.ToString(), Is.EqualTo("private, no-cache"));
            Assert.That(controller.Response.Headers.Vary.ToString(), Is.EqualTo("Cookie"));
        }
    }
}
