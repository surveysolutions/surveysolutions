using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class OptionView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        [Indexed]
        public string  QuestionnaireId { get; set; }

        [Indexed]
        public string QuestionId { get; set; }

        public decimal Value { get; set; }

        [Indexed]
        public string Title { get; set; }

        [Indexed]
        public string SearchTitle { get; set; }

        public decimal? ParentValue { get; set; }

        public int SortOrder { get; set; }

        public string TranslationId { get; set; }

        [Indexed]
        public string CategoryId { get; set; }
    }
}
