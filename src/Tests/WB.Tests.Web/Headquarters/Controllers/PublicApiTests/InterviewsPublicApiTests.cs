using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class InterviewsPublicApiTests : ApiTestContext
    {
        [Test] 
        public void when_user_is_not_authenticated_without_cookie_should_receive_forbid_for_pdf ()
        {
            var interviewId = Id.g1;
            var interview = Mock.Of<IStatefulInterview>();
            var pdfContent = new byte[] { 1, 2, 3, 4, 5};
            var pdfStream = new MemoryStream(pdfContent);

            var pdfGenerator = Mock.Of<IPdfInterviewGenerator>(
                x => x.Generate(interviewId) == pdfStream);
            var authorizedUser = Mock.Of<IAuthorizedUser>(
                x => x.IsAuthenticated == false);
            var statefulInterviewRepository = Mock.Of<IStatefulInterviewRepository>(
                x => x.Get(interviewId.ToString()) == interview);

            var controller = CreateInterviewsController(
                statefulInterviewRepository: statefulInterviewRepository,
                authorizedUser: authorizedUser, 
                pdfInterviewGenerator: pdfGenerator);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    Session = Mock.Of<ISession>()
                }
            };

            var actionResult = controller.Pdf(interviewId);
            
            Assert.That(actionResult is ForbidResult, Is.True);
        }
        
        [Test] 
        public void when_interviewer_try_access_to_interview_in_other_team_should_receive_forbid_for_pdf ()
        {
            var interviewId = Id.g1;
            var interview = Create.AggregateRoot.StatefulInterview(interviewId, userId: Id.g3);
            var pdfContent = new byte[] { 1, 2, 3, 4, 5};
            var pdfStream = new MemoryStream(pdfContent);

            var pdfGenerator = Mock.Of<IPdfInterviewGenerator>(
                x => x.Generate(interviewId) == pdfStream);
            var authorizedUser = Mock.Of<IAuthorizedUser>(
                x => x.IsAuthenticated == true && x.IsInterviewer == true && x.Id == Id.g2);
            var statefulInterviewRepository = Mock.Of<IStatefulInterviewRepository>(
                x => x.Get(interviewId.ToString()) == interview);

            var controller = CreateInterviewsController(
                statefulInterviewRepository: statefulInterviewRepository,
                authorizedUser: authorizedUser, 
                pdfInterviewGenerator: pdfGenerator);

            var actionResult = controller.Pdf(interviewId);
            
            Assert.That(actionResult is ForbidResult, Is.True);
        }

        [Test] 
        public void when_supervisor_try_access_to_interview_in_other_team_should_receive_forbid_for_pdf ()
        {
            var interviewId = Id.g1;
            var interview = Create.AggregateRoot.StatefulInterview(interviewId, supervisorId: Id.g3);
            var pdfContent = new byte[] { 1, 2, 3, 4, 5};
            var pdfStream = new MemoryStream(pdfContent);

            var pdfGenerator = Mock.Of<IPdfInterviewGenerator>(
                x => x.Generate(interviewId) == pdfStream);
            var authorizedUser = Mock.Of<IAuthorizedUser>(
                x => x.IsAuthenticated == true && x.IsSupervisor == true && x.Id == Id.g2);
            var statefulInterviewRepository = Mock.Of<IStatefulInterviewRepository>(
                x => x.Get(interviewId.ToString()) == interview);

            var controller = CreateInterviewsController(
                statefulInterviewRepository: statefulInterviewRepository,
                authorizedUser: authorizedUser, 
                pdfInterviewGenerator: pdfGenerator);

            var actionResult = controller.Pdf(interviewId);
            
            Assert.That(actionResult is ForbidResult, Is.True);
        }
        
        [Test] 
        public void when_user_has_access_to_interview_should_receive_pdf ()
        {
            var interviewId = Id.g1;
            var interview = Mock.Of<IStatefulInterview>(x => x.GetInterviewKey() == new InterviewKey(12345678));
            var pdfContent = new byte[] { 1, 2, 3, 4, 5};
            var pdfStream = new MemoryStream(pdfContent);

            var pdfGenerator = Mock.Of<IPdfInterviewGenerator>(
                x => x.Generate(interviewId) == pdfStream);
            var authorizedUser = Mock.Of<IAuthorizedUser>(
                x => x.IsAuthenticated == true && x.IsAdministrator && x.Id == Id.g2);
            var statefulInterviewRepository = Mock.Of<IStatefulInterviewRepository>(
                x => x.Get(interviewId.ToString()) == interview);

            var controller = CreateInterviewsController(
                statefulInterviewRepository: statefulInterviewRepository,
                authorizedUser: authorizedUser, 
                pdfInterviewGenerator: pdfGenerator);

            var actionResult = controller.Pdf(interviewId);

            var fileResult = actionResult as FileStreamResult;
            Assert.That(fileResult, Is.Not.Null);
            Assert.That(fileResult.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(fileResult.FileDownloadName, Is.EqualTo("12-34-56-78.pdf"));
        }
    }
}