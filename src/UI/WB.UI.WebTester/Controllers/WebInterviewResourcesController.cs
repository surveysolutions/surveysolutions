﻿using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Services;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    [Route("api/media")]
    public class WebInterviewResourcesController : Controller
    {
        private readonly ICacheStorage<QuestionnaireAttachment, string> attachmentStorage;
        private readonly IImageProcessingService imageProcessingService;
        private readonly ICacheStorage<MultimediaFile, string> mediaStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;

        public WebInterviewResourcesController(
            ICacheStorage<QuestionnaireAttachment, string> attachmentStorage,
            IImageProcessingService imageProcessingService,
            ICacheStorage<MultimediaFile, string> mediaStorage,
            IStatefulInterviewRepository statefulInterviewRepository)
        {
            this.attachmentStorage = attachmentStorage ?? throw new ArgumentNullException(nameof(attachmentStorage));
            this.imageProcessingService = imageProcessingService ?? throw new ArgumentNullException(nameof(imageProcessingService));
            this.mediaStorage = mediaStorage ?? throw new ArgumentNullException(nameof(mediaStorage));
            this.statefulInterviewRepository = statefulInterviewRepository ?? throw new ArgumentNullException(nameof(statefulInterviewRepository));
        }

        [HttpHead]
        [Route("content")]
        public IActionResult ContentHead([FromQuery] string interviewId, [FromQuery] string contentId)
        {
            var attachment = attachmentStorage.Get(contentId, Guid.Parse(interviewId));
            if (attachment == null)
            {
                return NoContent();
            }

            var stream = new MemoryStream(attachment.Content.Content);
            return File(stream, attachment.Content.ContentType, enableRangeProcessing: true);
        }

        [HttpGet]
        [Route("content")]
        public IActionResult GetContent([FromQuery] string interviewId, [FromQuery] string contentId)
        {
            var attachment = attachmentStorage.Get(contentId, Guid.Parse(interviewId));
            if (attachment == null)
            {
                return NotFound();
            }

            if (attachment.Content.IsImage())
            {
                var fullSize = GetQueryStringValue("fullSize") != null;

                var resultFile = fullSize
                    ? attachment.Content.Content
                    : this.imageProcessingService.ResizeImage(attachment.Content.Content, 200, 1920);
                
                return this.BinaryResponseMessageWithEtag(resultFile);
            }

            MemoryStream stream = new MemoryStream(attachment.Content.Content);

            return File(stream, attachment.Content.ContentType, enableRangeProcessing: true);
        }

        [HttpGet]
        [Route("image")]
        public IActionResult Image([FromQuery] string interviewId, [FromQuery] string questionId,
            [FromQuery] string filename)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);

            var file = this.mediaStorage.Get(filename, interview.Id);

            if ((file?.Data?.Length ?? 0) == 0)
                return NoContent();

            var fullSize = GetQueryStringValue("fullSize") != null;
            var resultFile = fullSize
                ? file.Data
                : this.imageProcessingService.ResizeImage(file.Data, 200, 1920);
            
            return this.BinaryResponseMessageWithEtag(resultFile);
        }

        private string GetQueryStringValue(string key)
        {
            return (this.Request.Query.Where(query => query.Key == key).Select(query => query.Value))
                .FirstOrDefault();
        }
    }
}
