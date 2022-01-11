using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.WebInterview;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class WebInterviewIndexPageModel
    {
        public string Id { get; set; }
        public Dictionary<WebInterviewUserMessages, string> CustomMessages { get; set; }
        public string AskForEmail { get; set; }
        public string CoverPageId { get; set; }
        public bool MayBeSwitchedToWebMode { get; set; }
        public string WebInterviewUrl { get; set; }
    }
}
