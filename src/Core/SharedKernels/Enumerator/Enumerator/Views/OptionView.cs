using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class OptionView : IPlainStorageEntity<int?>
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        [Indexed(Name = "entity_idx", Order = 1)]
        public string  QuestionnaireId { get; set; }

        [Indexed(Name = "entity_idx", Order = 2)]
        public string QuestionId { get; set; }

        [Indexed(Name = "entity_idx", Order = 3)]
        public string CategoryId { get; set; }

        [Indexed(Name = "entity_idx", Order = 4)]
        public string TranslationId { get; set; }

        [Indexed(Name = "OptionView_Value")]
        public decimal Value { get; set; }

        public string Title { get; set; }

        public string SearchTitle { get; set; }

        [Indexed(Name = "OptionView_ParentValue")]
        public decimal? ParentValue { get; set; }

        public int SortOrder { get; set; }
        public string AttachmentName { get; set; }
    }
}
