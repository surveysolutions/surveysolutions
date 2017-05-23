using System.Collections.Generic;
using MvvmCross.Platform;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class AssignmentDocument : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        public int? Capacity { get; set; }
        public int Created { get; set; }

        [Ignore]
        public QuestionnaireIdentity QuestionnaireId
        {
            get => QuestionnaireIdentity.Parse(QuestionnaireIdValue);
            set => this.QuestionnaireIdValue = value.ToString();
        }

        public string QuestionnaireIdValue { get; set; }

        [Ignore]
        public List<IdentifyingAnswer> IdentifyingData
        {
            get => Mvx.Resolve<IJsonAllTypesSerializer>().Deserialize<List<IdentifyingAnswer>>(Data);
            set => Data = Mvx.Resolve<IJsonAllTypesSerializer>().SerializeToByteArray(value);
        }

        public byte[] Data { get; set; }
    }
}