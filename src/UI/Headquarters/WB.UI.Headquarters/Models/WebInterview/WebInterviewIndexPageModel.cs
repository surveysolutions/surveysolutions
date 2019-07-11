using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.WebInterview;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class WebInterviewIndexPageModel
    {
        public Dictionary<WebInterviewUserMessages, string> CustomMessages { get; set; }
        public string AskForEmail { get; set; }
    }
}
