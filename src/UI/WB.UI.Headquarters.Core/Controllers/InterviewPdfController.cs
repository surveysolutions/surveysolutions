using System;
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

            var content = interviewGenerator.Generate(interviewId);
            if (content == null)
                return NotFound(interviewId);

            return this.File(content, 
                "application/pdf", 
                fileDownloadName: interview.GetInterviewKey() + ".pdf");
        }

    }
}