using System;
using System.Collections.Generic;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using Newtonsoft.Json;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class AssignmentDocument : IPlainStorageEntity
    {
        [Ignore]
        public List<IdentifyingAnswer> IdentifyingData
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.IdentifyingDataValue)
                    ? null
                    : JsonConvert.DeserializeObject<List<IdentifyingAnswer>>(this.IdentifyingDataValue);
            }
            set { this.IdentifyingDataValue = JsonConvert.SerializeObject(value); }
        }

        public string IdentifyingDataValue { get; set; }

        [PrimaryKey]
        public virtual string Id { get; set; }

        public int? Quantity { get; set; }

        public int InterviewsCount { get; set; }

        public string QuestionnaireId { get; set; }

        public string Title { get; set; }

        public Guid? LocationQuestionId { get; set; }

        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }

        public class IdentifyingAnswer
        {
            public string Answer { get; set; }
            public Guid QuestionId { get; set; }
            public string Question { get; set; }
        }
    }
}