using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WB.UI.Headquarters.PdfInterview;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Headquarters.Controllers
{
    public class InterviewPdfController : Controller
    {
        private readonly IPdfInterviewGenerator interviewGenerator;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;

        public InterviewPdfController(IPdfInterviewGenerator interviewGenerator,
            IStatefulInterviewRepository statefulInterviewRepository)
        {
            this.interviewGenerator = interviewGenerator;
            this.statefulInterviewRepository = statefulInterviewRepository;
        }
        
        public IActionResult PdfPrint(Guid interviewId)
        {
            var interview = statefulInterviewRepository.Get(interviewId.FormatGuid());
            if (interview == null)
                return NotFound(interviewId);

            var content = interviewGenerator.Generate(interviewId, User);
            if (content == null)
                return NotFound(interviewId);
            
            var isExistsInterviewInCookie = Request.Cookies.Keys.Where(key => key.StartsWith($"InterviewId-"))
                .Any(key =>
                    Guid.TryParse(Request.Cookies[key], out Guid cookieInterviewId)
                    && cookieInterviewId == interview.Id
                );
            var hasAccess = isExistsInterviewInCookie && interview.CompletedDate.HasValue
                                                      && interview.CompletedDate.Value.AddHours(1) > DateTime.UtcNow;
            if (!hasAccess)
                return Forbid();

            return this.File(content, 
                "application/pdf", 
                fileDownloadName: interview.GetInterviewKey() + ".pdf");
        }
    }
}