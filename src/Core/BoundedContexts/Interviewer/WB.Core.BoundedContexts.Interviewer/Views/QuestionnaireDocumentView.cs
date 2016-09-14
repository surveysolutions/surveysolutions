using Main.Core.Documents;
using MvvmCross.Platform;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class QuestionnaireDocumentView : IPlainStorageEntity
    {

        [PrimaryKey]
        public string Id { get; set; }

        [Ignore]
        public QuestionnaireDocument QuestionnaireDocument
        {
            get { return Mvx.Resolve<IJsonAllTypesSerializer>().Deserialize<QuestionnaireDocument>(Document); }
            set { Document = Mvx.Resolve<IJsonAllTypesSerializer>().SerializeToByteArray(value); }
        }

        public byte[] Document { get; set; }
    }
}
