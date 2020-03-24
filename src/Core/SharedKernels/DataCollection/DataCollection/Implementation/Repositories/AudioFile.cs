﻿using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    public class AudioFile : IView
    {
        public virtual string Id { get; set; }

        public virtual Guid InterviewId { get; set; }

        public virtual string FileName { get; set; }

        public virtual byte[] Data { get; set; }

        public virtual string ContentType { get; set; }

        public static string GetFileId(Guid interviewId, string fileName) => $"{interviewId}#{fileName}";
    }
}
