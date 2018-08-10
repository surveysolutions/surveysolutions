using Newtonsoft.Json;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class InterviewerApplicationPatchApiView
    {
        [JsonProperty("name")]
        public string FileName { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("size")]
        public long SizeInBytes { get; set; }
    }
}
