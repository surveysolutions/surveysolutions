using Main.Core.Documents;
using MvvmCross;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class QuestionnaireDocumentView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        [Ignore]
        public QuestionnaireDocument QuestionnaireDocument
        {
            get => Mvx.Resolve<IJsonAllTypesSerializer>().Deserialize<QuestionnaireDocument>(Document);
            set => Document = Mvx.Resolve<IJsonAllTypesSerializer>().SerializeToByteArray(value);
        }

        public byte[] Document { get; set; }
    }
}
