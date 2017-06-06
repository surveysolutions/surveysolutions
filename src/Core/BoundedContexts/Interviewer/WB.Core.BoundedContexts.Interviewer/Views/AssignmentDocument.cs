using System;
using System.Collections.Generic;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using Newtonsoft.Json;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class AssignmentDocument : IPlainStorageEntity
    {
        [SQLite.Ignore]
        public List<IdentifyingAnswer> IdentifyingData { get; set; }

        public string IdentifyingDataValue
        {
            get => JsonConvert.SerializeObject(IdentifyingData);

            set => IdentifyingData = string.IsNullOrWhiteSpace(value)
                ? new List<IdentifyingAnswer>()
                : JsonConvert.DeserializeObject<List<IdentifyingAnswer>>(value);
        }

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