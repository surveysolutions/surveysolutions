using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.UI.Headquarters.Implementation.Maps
{
    public class JsonMapResponse : JsonBaseResponse
    {
        public List<string> Errors { get; set; } = new List<string>(); 
    }
}
