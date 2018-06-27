using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class AssignmentApiDocument
    {
        public int Id { get; set; }

        public QuestionnaireIdentity QuestionnaireId { get; set; }

        public int? Quantity { get; set; }

        public List<InterviewSerializedAnswer> Answers { get; set; } = new List<InterviewSerializedAnswer>();

        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }
        public Guid? LocationQuestionId { get; set; }

        public Guid ResponsibleId { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public List<string> ProtectedVariables { get; set; }

        public class InterviewSerializedAnswer
        {
            [JsonProperty("id")]
            public Identity Identity { get; set; }

            [JsonProperty("val")]
            public string SerializedAnswer { get; set; }
        }
    }

    public class AssignmentApiView
    {
        public int Id { get; set; }

        [JsonProperty("qid")]
        public QuestionnaireIdentity QuestionnaireId { get; set; }

        public int? Quantity { get; set; }
    }
}
