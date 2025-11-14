using System;
using System.IO;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public class PostFileRequest
    {
        public Guid InterviewId { get; set; }
        
        private string? fileName;
        public string? FileName
        {
            get => fileName;
            set => fileName = string.IsNullOrEmpty(value) ? null : Path.GetFileName(value);
        }
        public string? Data { get; set; }
        public string? ContentType { get; set; }
    }
}
